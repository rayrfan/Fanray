using Fan.Data;
using Fan.Exceptions;
using Fan.Extensibility;
using Fan.Helpers;
using Fan.Settings;
using Fan.Themes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("Fan.IntegrationTests")]

namespace Fan.Widgets
{
    /// <summary>
    /// The widget service.
    /// </summary>
    public class WidgetService : ExtensibleService<WidgetManifest, Widget>, IWidgetService
    {
        public static WidgetAreaInfo BlogSidebar1 = new WidgetAreaInfo { Id = "blog-sidebar1", Name = "Blog - Sidebar1" };
        public static WidgetAreaInfo BlogSidebar2 = new WidgetAreaInfo { Id = "blog-sidebar2", Name = "Blog - Sidebar2" };
        public static WidgetAreaInfo BlogBeforePost = new WidgetAreaInfo { Id = "blog-before-post", Name = "Blog - Before Post" };
        public static WidgetAreaInfo BlogAfterPost = new WidgetAreaInfo { Id = "blog-after-post", Name = "Blog - After Post" };
        public static WidgetAreaInfo BlogBeforePostList = new WidgetAreaInfo { Id = "blog-before-post-list", Name = "Blog - Before Post List" };
        public static WidgetAreaInfo BlogAfterPostList = new WidgetAreaInfo { Id = "blog-after-post-list", Name = "Blog - After Post List" };
        public static WidgetAreaInfo Footer1 = new WidgetAreaInfo { Id = "footer1", Name = "Footer 1" };
        public static WidgetAreaInfo Footer2 = new WidgetAreaInfo { Id = "footer2", Name = "Footer 2" };
        public static WidgetAreaInfo Footer3 = new WidgetAreaInfo { Id = "footer3", Name = "Footer 3" };

        public static IEnumerable<WidgetAreaInfo> SystemDefinedWidgetAreaInfos = new List<WidgetAreaInfo>
        {
            BlogSidebar1,
            BlogSidebar2,
            BlogBeforePost,
            BlogAfterPost,
            BlogBeforePostList,
            BlogAfterPostList,
            Footer1,
            Footer2,
            Footer3,
        };

        /// <summary>
        /// The widget manifest file name.
        /// </summary>
        public const string WIDGET_MANIFEST = "widget.json";
        /// <summary>
        /// The widgets directory inside the web app.
        /// </summary>
        public const string WIDGETS_DIR = "Widgets";
        /// <summary>
        /// A widget's folder must be in PascalCase.
        /// </summary>
        /// <remarks>
        /// https://stackoverflow.com/a/2106423/32240
        /// </remarks>
        public const string WIDGET_FOLDER_REGEX = @"^[A-Z][a-z]+(?:[A-Z][a-z]+)*$";

        private const string CACHE_KEY_CURRENT_THEME_AREAS = "{0}-theme-widget-areas";
        private TimeSpan Cache_Time_Current_Theme_Areas = new TimeSpan(0, 10, 0);
        private const string CACHE_KEY_WIDGETS_MANIFESTS = "installed-widgets-manifests";
        private TimeSpan Cache_Time_Widgets_Manifests = new TimeSpan(0, 10, 0);

        private readonly IThemeService themeService;
        private readonly ISettingService settingService;

        public WidgetService(IMetaRepository metaRepository,
            IThemeService themeService,
            IDistributedCache distributedCache,
            ISettingService settingService,
            IHostingEnvironment hostingEnvironment,
            ILogger<WidgetService> logger)
            : base(metaRepository, distributedCache, hostingEnvironment, logger)
        {
            this.themeService = themeService;
            this.settingService = settingService;
        }

        public override string ManifestName { get; } = WIDGET_MANIFEST;
        public override string ManifestDirectory { get; } = WIDGETS_DIR;

        // -------------------------------------------------------------------- widget areas

        /// <summary>
        /// Registers a widget area by its id and type.
        /// </summary>
        /// <param name="areaId">The id of the widget area.</param>
        /// <param name="type">The <see cref="EMetaType"/> of the area.</param>
        /// <returns></returns>
        public async Task<Meta> RegisterAreaAsync(string areaId, EMetaType type = EMetaType.WidgetAreaBySystem)
        {
            var key = areaId;
            if (IsThemeDefinedArea(areaId))
            {
                key = await GetThemeAreaMetaKeyAsync(areaId);
                type = EMetaType.WidgetAreaByTheme;
            }

            return await metaRepository.CreateAsync(new Meta
            {
                Key = key,
                Value = JsonConvert.SerializeObject(new WidgetArea { Id = areaId }),
                Type = type,
            });
        }

        /// <summary>
        /// Returns a widget area by id from current theme's areas.
        /// Returns null if no area by id is found.
        /// </summary>
        /// <param name="areaId"></param>
        /// <returns></returns>
        public async Task<WidgetAreaInstance> GetAreaAsync(string areaId)
        {
            var list = await GetCurrentThemeAreasAsync();
            return list.SingleOrDefault(a => a.Id.Equals(areaId, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Returns a list of <see cref="WidgetAreaInstance"/> for the current theme, 
        /// each area contains its a list of <see cref="WidgetInstance"/>.
        /// </summary>
        /// <remarks>
        /// This method has a side effect of registering a theme defined widget area,
        /// that happens when a user adds an area to the theme on the fly.
        /// </remarks>
        public async Task<IEnumerable<WidgetAreaInstance>> GetCurrentThemeAreasAsync()
        {
            var coreSettings = await settingService.GetSettingsAsync<CoreSettings>();

            var cacheKey = string.Format(CACHE_KEY_CURRENT_THEME_AREAS, coreSettings.Theme);
            return await distributedCache.GetAsync(cacheKey, Cache_Time_Current_Theme_Areas, async () =>
            {
                var widgetAreaInstancelist = new List<WidgetAreaInstance>();

                var currentTheme = (await themeService.GetManifestsAsync())
                                   .Single(t => t.Name.Equals(coreSettings.Theme, StringComparison.OrdinalIgnoreCase));
                foreach (var areaInfo in currentTheme.WidgetAreas)
                {
                    var metaArea = await GetAreaMetaAsync(areaInfo.Id);
                    if (metaArea == null)
                    {
                        // if not found this should be a newly added theme-defined area
                        metaArea = await RegisterAreaAsync(areaInfo.Id, EMetaType.WidgetAreaByTheme);
                    }
                    var widgetArea = JsonConvert.DeserializeObject<WidgetArea>(metaArea.Value);

                    var widgetAreaInstance = new WidgetAreaInstance {
                        Id = widgetArea.Id,
                        Title = GetAreaName(widgetArea.Id, areaInfo.Name),
                        WidgetIds = widgetArea.WidgetIds,
                    };

                    foreach (var id in widgetArea.WidgetIds)
                    {
                        var widget = await GetExtensionAsync(id);
                        var widgetManifest = await GetManifestByFolderAsync(widget.Folder);
                        var widgetInstance = new WidgetInstance {
                            Id = id,
                            Title = widget.Title,
                            Name = widgetManifest.Name,
                            Folder = widget.Folder,
                            AreaId = areaInfo.Id,
                        };

                        widgetAreaInstance.Widgets.Add(widget);
                        widgetAreaInstance.WidgetInstances.Add(widgetInstance);
                    }

                    widgetAreaInstancelist.Add(widgetAreaInstance);
                }

                return widgetAreaInstancelist;
            }, includeTypeName: true);
        }

        // -------------------------------------------------------------------- manifests

        /// <summary>
        /// Returns a list of widget manifests.
        /// </summary>
        public override async Task<IEnumerable<WidgetManifest>> GetManifestsAsync()
        {
            return await distributedCache.GetAsync(CACHE_KEY_WIDGETS_MANIFESTS, Cache_Time_Widgets_Manifests, async () =>
            {
                return await LoadManifestsAsync();
            });
        }

        // -------------------------------------------------------------------- add / remove / order

        /// <summary>
        /// Adds a widget instance to a widget area.
        /// </summary>
        /// <param name="widgetId">Id of the widget to add to the area.</param>
        /// <param name="areaId">Id of the area the widget is added to.</param>
        /// <param name="index">Index of the widget in the area's widgets id array.</param>
        /// <returns></returns>
        public async Task<WidgetInstance> AddWidgetToAreaAsync(int widgetId, string areaId, int index)
        {
            // get area
            var metaArea = await GetAreaMetaAsync(areaId); // metaRepository.GetAsync(areaId, type);
            var area = JsonConvert.DeserializeObject<WidgetArea>(metaArea.Value);

            // insert new id to area
            List<int> widgetIdsList = area.WidgetIds.ToList();
            widgetIdsList.Insert(index, widgetId);
            area.WidgetIds = widgetIdsList.ToArray();

            // update widget instance id and areaId
            var widget = await GetExtensionAsync(widgetId);
            widget.Id = widgetId;
            widget.AreaId = areaId;
            await UpdateWidgetAsync(widgetId, widget);

            // update meta
            metaArea.Value = JsonConvert.SerializeObject(area);
            await metaRepository.UpdateAsync(metaArea);

            // invalidate cache
            await InvalidAreaCacheAsync();

            var widgetInfo = await GetManifestByFolderAsync(widget.Folder);
            return new WidgetInstance
            {
                Id = widgetId,
                Title = widget.Title,
                Name = widgetInfo.Name,
                Folder = widget.Folder,
                AreaId = areaId,
            };
        }

        /// <summary>
        /// Removes a widget instance from a widget area.
        /// </summary>
        /// <param name="widgetId"></param>
        /// <param name="areaId"></param>
        public async Task RemoveWidgetFromAreaAsync(int widgetId, string areaId)
        {
            // get the area by key
            var metaArea = await GetAreaMetaAsync(areaId); // metaRepository.GetAsync(areaId, type);
            var widgetArea = JsonConvert.DeserializeObject<WidgetArea>(metaArea.Value);

            // delete the id from area's id array
            widgetArea.WidgetIds = widgetArea.WidgetIds.Where(id => id != widgetId).ToArray();

            // update the area
            metaArea.Value = JsonConvert.SerializeObject(widgetArea);
            await metaRepository.UpdateAsync(metaArea);

            // invalidate areas from cache
            await InvalidAreaCacheAsync();
        }

        /// <summary>
        /// Moves a widget in an area to a new position.
        /// </summary>
        /// <param name="widgetId"></param>
        /// <param name="areaId"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public async Task OrderWidgetInAreaAsync(int widgetId, string areaId, int index)
        {
            // get area
            var metaArea = await GetAreaMetaAsync(areaId); // await metaRepository.GetAsync(areaId);
            var area = JsonConvert.DeserializeObject<WidgetArea>(metaArea.Value);

            // reorder the widget in area
            var widgetIdsList = area.WidgetIds.ToList();
            widgetIdsList.Remove(widgetId);
            widgetIdsList.Insert(index, widgetId);
            area.WidgetIds = widgetIdsList.ToArray();

            // update meta
            metaArea.Value = JsonConvert.SerializeObject(area);
            await metaRepository.UpdateAsync(metaArea);

            // invalidate cache
            await InvalidAreaCacheAsync();
        }

        // -------------------------------------------------------------------- create / delete / update

        /// <summary>
        /// Creates a widget instance. 
        /// </summary>
        /// <param name="widgetType"></param>
        /// <returns></returns>
        /// <remarks>
        /// The newly created instance has the widget's default values.
        /// </remarks>
        public async Task<int> CreateWidgetAsync(string folder)
        {
            var type = await GetManifestTypeByFolderAsync(folder);
            var widget = (Widget)Activator.CreateInstance(type);
            return await CreateWidgetAsync(widget, folder);
        }

        /// <summary>
        /// Creates a widget instance by a given widget object.
        /// </summary>
        /// <param name="widget"></param>
        /// <param name="folder"></param>
        /// <returns></returns>
        /// <remarks>
        /// The widget object has the widget's seed values.
        /// </remarks>
        public async Task<int> CreateWidgetAsync(Widget widget, string folder)
        {
            widget.Folder = folder;
            Meta metaWidget = null;
            while (metaWidget == null)
            {
                try
                {
                    // create widget meta record
                    metaWidget = await metaRepository.CreateAsync(new Meta
                    {
                        Key = string.Format($"{folder}-{Util.RandomString(6)}").ToLower(),
                        Value = JsonConvert.SerializeObject(widget),
                        Type = EMetaType.Widget,
                    });
                }
                catch (FanException ex) when (ex.ExceptionType == EExceptionType.MetaDuplicate)
                {
                }
            }

            return metaWidget.Id;
        }

        /// <summary>
        /// Deletes a widget instance.
        /// </summary>
        /// <param name="widgetId"></param>
        /// <returns></returns>
        public async Task DeleteWidgetAsync(int widgetId)
        {
            await metaRepository.DeleteAsync(widgetId);
        }

        /// <summary>
        /// Updates a widget instance.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="widget"></param>
        /// <returns></returns>
        public async Task UpdateWidgetAsync(int id, Widget widget)
        {
            var meta = await metaRepository.GetAsync(id);
            meta.Value = JsonConvert.SerializeObject(widget);
            await metaRepository.UpdateAsync(meta);
            await InvalidAreaCacheAsync();
        }

        // -------------------------------------------------------------------- validate

        public override bool IsValidExtensionFolder(string folder) => new Regex(WIDGET_FOLDER_REGEX).IsMatch(folder);

        // -------------------------------------------------------------------- private methods

        /// <summary>
        /// Returns a <see cref="Meta"/> area record that is either by system or by theme.
        /// </summary>
        /// <param name="areaId"></param>
        /// <returns></returns>
        private async Task<Meta> GetAreaMetaAsync(string areaId)
        {
            var key = areaId;
            var type = EMetaType.WidgetAreaBySystem;
            if (IsThemeDefinedArea(areaId))
            {
                key = await GetThemeAreaMetaKeyAsync(areaId);
                type = EMetaType.WidgetAreaByTheme;
            }

            return await metaRepository.GetAsync(key, type);
        }

        /// <summary>
        /// Returns true if the area id is a system-defined widget area.
        /// </summary>
        /// <param name="areaId"></param>
        /// <returns></returns>
        private bool IsThemeDefinedArea(string areaId)
        {
            return !SystemDefinedWidgetAreaInfos.Any(sa => sa.Id.Equals(areaId, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Returns the meta key for a theme-defined widget area. 
        /// Theme-defined widget area meta key is prefixed by current theme and all lower case.
        /// </summary>
        /// <param name="areaId"></param>
        /// <returns></returns>
        private async Task<string> GetThemeAreaMetaKeyAsync(string areaId)
        {
            var coreSettings = await settingService.GetSettingsAsync<CoreSettings>();
            return string.Format($"{coreSettings.Theme}-{areaId}").ToLower();
        }

        /// <summary>
        /// Returns an area's display name for Admin Panel Widgets page.
        /// </summary>
        /// <param name="areaId"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <remarks>
        /// If user didn't provide a name for a widget area, if the area is theme defined its id is returned,
        /// if the area is system defined the default name is returned.
        /// </remarks>
        private string GetAreaName(string areaId, string name)
        {
            if (!name.IsNullOrEmpty()) return name;
            var areaInfo = SystemDefinedWidgetAreaInfos.Single(a => a.Id.Equals(areaId, StringComparison.OrdinalIgnoreCase));
            return areaInfo == null ? areaId : areaInfo.Name;
        }

        /// <summary>
        /// Invalidates areas cache.
        /// </summary>
        /// <returns></returns>
        private async Task InvalidAreaCacheAsync()
        {
            // invalidate areas from cache
            var coreSettings = await settingService.GetSettingsAsync<CoreSettings>();
            var cacheKey = string.Format(CACHE_KEY_CURRENT_THEME_AREAS, coreSettings.Theme);
            await distributedCache.RemoveAsync(cacheKey);
        }

        /// <summary>
        /// Returns the path to widget view file which is located in Fan.Web/Widgets folder.
        /// </summary>
        /// <param name="widgetName"></param>
        /// <returns></returns>
        public static string GetWidgetViewPath(string widgetName) => $"~/{WIDGETS_DIR}/{widgetName}/{widgetName}.cshtml";
    }
}

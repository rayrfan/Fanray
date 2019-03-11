using Fan.Data;
using Fan.Settings;
using Fan.Themes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.IO;

namespace Fan.Widgets
{
    public class WidgetService : IWidgetService
    {
        public static WidgetArea BlogSidebar1 = new WidgetArea { Id = "blog-sidebar1", Title = "Blog - Sidebar1" };
        public static WidgetArea BlogSidebar2 = new WidgetArea { Id = "blog-sidebar2", Title = "Blog - Sidebar2" };
        public static WidgetArea BlogBeforePost = new WidgetArea { Id = "blog-beforepost", Title = "Blog - Before Post" };
        public static WidgetArea BlogAfterPost = new WidgetArea { Id = "blog-afterpost", Title = "Blog - After Post" };
        public static WidgetArea Footer1 = new WidgetArea { Id = "footer1", Title = "Footer 1" };
        public static WidgetArea Footer2 = new WidgetArea { Id = "footer2", Title = "Footer 2" };
        public static WidgetArea Footer3 = new WidgetArea { Id = "footer3", Title = "Footer 3" };

        public const string WIDGET_INFO_FILE_NAME = "widget.json";
        public string WidgetDirectoryName { get; set; } = $"Pages{Path.DirectorySeparatorChar}Widgets";

        private readonly IMetaRepository metaRepository;
        private readonly IThemeService themeService;
        private readonly IDistributedCache distributedCache;
        private readonly ISettingService settingService;
        private readonly IHostingEnvironment hostingEnvironment;
        private readonly ILogger<WidgetService> logger;

        public WidgetService(IMetaRepository metaRepository,
            IThemeService themeService,
            IDistributedCache distributedCache,
            ISettingService settingService,
            IHostingEnvironment hostingEnvironment,
            ILogger<WidgetService> logger)
        {
            this.metaRepository = metaRepository;
            this.themeService = themeService;
            this.distributedCache = distributedCache;
            this.settingService = settingService;
            this.hostingEnvironment = hostingEnvironment;
            this.logger = logger;
        }

        private const string CACHE_KEY_CURRENT_THEME_AREAS = "{0}-theme-widget-areas";
        private TimeSpan Cache_Time_Current_Theme_Areas = new TimeSpan(0, 10, 0);
        private const string CACHE_KEY_INSTALLED_WIDGETS_INFO = "installed-widgets-info";
        private TimeSpan Cache_Time_Installed_Widgets_Info = new TimeSpan(0, 10, 0);

        // -------------------------------------------------------------------- widget areas

        /// <summary>
        /// Register a widget area during setup.
        /// </summary>
        /// <param name="area"></param>
        /// <returns></returns>
        public async Task RegisterAreaAsync(WidgetArea area)
        {
            await metaRepository.CreateAsync(new Meta
            {
                Key = area.Id,
                Value = JsonConvert.SerializeObject(area),
                Type = EMetaType.WidgetArea,
            });
        }

        /// <summary>
        /// Returns a widget area by id from current theme's areas.
        /// </summary>
        /// <param name="areaId"></param>
        /// <returns></returns>
        public async Task<WidgetAreaInstance> GetAreaAsync(string areaId)
        {
            var list = await GetCurrentThemeAreasAsync();
            return list.Single(a => a.Id.Equals(areaId, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Returns a list of <see cref="WidgetAreaInstance"/> for the current theme, 
        /// each area has the a list of <see cref="WidgetInstance"/> it contains.
        /// </summary>
        public async Task<IEnumerable<WidgetAreaInstance>> GetCurrentThemeAreasAsync()
        {
            var coreSettings = await settingService.GetSettingsAsync<CoreSettings>();

            var cacheKey = string.Format(CACHE_KEY_CURRENT_THEME_AREAS, coreSettings.Theme);
            return await distributedCache.GetAsync(cacheKey, Cache_Time_Current_Theme_Areas, async () =>
            {
                var widgetAreaInstancelist = new List<WidgetAreaInstance>();

                var themeInfos = await themeService.GetInstalledThemesInfoAsync();
                var currentTheme = themeInfos.Single(t => t.Name.Equals(coreSettings.Theme, StringComparison.OrdinalIgnoreCase));
                var areaIds = currentTheme.WidgetAreas; // current theme's widget areas ids

                foreach (var areaId in areaIds)
                {
                    var metaArea = await metaRepository.GetAsync(areaId);
                    var widgetArea = JsonConvert.DeserializeObject<WidgetArea>(metaArea.Value);

                    var widgetAreaInstance = new WidgetAreaInstance {
                        Id = widgetArea.Id,
                        Title = widgetArea.Title,
                        WidgetIds = widgetArea.WidgetIds,
                    };

                    foreach (var id in widgetArea.WidgetIds)
                    {
                        var widget = await GetWidgetAsync(id);
                        var widgetInfo = await GetWidgetInfoAsync(widget.Type);
                        var widgetInstance = new WidgetInstance {
                            Id = id,
                            Title = widget.Title,
                            Name = widgetInfo.Name,
                            Folder = widgetInfo.Folder,
                            AreaId = areaId,
                            Type = widget.Type,
                        };

                        widgetAreaInstance.Widgets.Add(widget);
                        widgetAreaInstance.WidgetInstances.Add(widgetInstance);
                    }

                    widgetAreaInstancelist.Add(widgetAreaInstance);
                }

                return widgetAreaInstancelist;
            }, includeTypeName: true);
        }

        // -------------------------------------------------------------------- widget infos

        /// <summary>
        /// Returns a list of widget info for all widgets found in "Fan.Web.Widgets" folder.
        /// </summary>
        /// <remarks>
        /// This method scans the "Fan.Web.Widgets" folder and reads all the "widget.json" files for each widget.
        /// Currently I don't save these data to db till download working.
        /// </remarks>
        public async Task<IEnumerable<WidgetInfo>> GetInstalledWidgetsInfoAsync()
        {
            return await distributedCache.GetAsync(CACHE_KEY_INSTALLED_WIDGETS_INFO, Cache_Time_Installed_Widgets_Info, async () =>
            { 
                var list = new List<WidgetInfo>();
                var widgetsFolder = Path.Combine(hostingEnvironment.ContentRootPath, WidgetDirectoryName);

                foreach (var dir in Directory.GetDirectories(widgetsFolder))
                {
                    var file = Path.Combine(dir, WIDGET_INFO_FILE_NAME);
                    var info = JsonConvert.DeserializeObject<WidgetInfo>(await File.ReadAllTextAsync(file));
                    info.Folder = new DirectoryInfo(dir).Name; // set folder
                    list.Add(info);
                }

                return list;
            });
        }

        /// <summary>
        /// Returns a <see cref="WidgetInfo"/> based on a given widget type.
        /// </summary>
        /// <param name="widgetType"></param>
        /// <returns></returns>
        public async Task<WidgetInfo> GetWidgetInfoAsync(string widgetType)
        {
            var widgetInfos = await GetInstalledWidgetsInfoAsync();
            return widgetInfos.Single(wi => wi.Type.Equals(widgetType, StringComparison.OrdinalIgnoreCase));
        }

        // -------------------------------------------------------------------- get / update

        /// <summary>
        /// Returns a <see cref="Widget"/> for update.
        /// </summary>
        /// <param name="widgetType"></param>
        /// <returns></returns>
        public async Task<Widget> GetWidgetAsync(int id)
        {
            var widgetMeta = await metaRepository.GetAsync(id);
            var widgetBase = (Widget)JsonConvert.DeserializeObject(widgetMeta.Value, typeof(Widget));

            var type = Type.GetType(widgetBase.Type);
            var widget = (Widget)JsonConvert.DeserializeObject(widgetMeta.Value, type);
            widget.Id = id;

            // the actual widget is returned
            return widget;
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
            var metaArea = await metaRepository.GetAsync(areaId);
            var area = JsonConvert.DeserializeObject<WidgetArea>(metaArea.Value);

            // insert new id to area
            List<int> widgetIdsList = area.WidgetIds.ToList();
            widgetIdsList.Insert(index, widgetId);
            area.WidgetIds = widgetIdsList.ToArray();

            // update widget instance id and areaId
            var widget = await GetWidgetAsync(widgetId);
            widget.Id = widgetId;
            widget.AreaId = areaId;
            await UpdateWidgetAsync(widgetId, widget);

            // update meta
            metaArea.Value = JsonConvert.SerializeObject(area);
            await metaRepository.UpdateAsync(metaArea);

            // invalidate cache
            await InvalidAreaCacheAsync();

            var widgetInfo = await GetWidgetInfoAsync(widget.Type);
            return new WidgetInstance
            {
                Id = widgetId,
                Title = widget.Title,
                Name = widgetInfo.Name,
                Folder = widgetInfo.Folder,
                AreaId = areaId,
                Type = widget.Type,
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
            var metaArea = await metaRepository.GetAsync(areaId);
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
            var metaArea = await metaRepository.GetAsync(areaId);
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

        // -------------------------------------------------------------------- create / delete

        /// <summary>
        /// Creates a widget instance by type. 
        /// </summary>
        /// <param name="widgetType"></param>
        /// <returns></returns>
        /// <remarks>
        /// The newly created instance has the widget's default values.
        /// </remarks>
        public async Task<int> CreateWidgetAsync(string widgetType)
        {
            var type = Type.GetType(widgetType);
            var widget = (Widget)Activator.CreateInstance(type);
            return await CreateWidgetAsync(widget, widgetType);
        }

        /// <summary>
        /// Creates a widget instance by a given widget object.
        /// </summary>
        /// <param name="widget"></param>
        /// <param name="widgetType"></param>
        /// <returns></returns>
        /// <remarks>
        /// The widget object has the widget's seed values.
        /// </remarks>
        public async Task<int> CreateWidgetAsync(Widget widget, string widgetType)
        {
            // add type info
            widget.Type = widgetType;

            // get widget info
            var widgetInfo = await GetWidgetInfoAsync(widget.Type);

            // create widget meta record
            var metaWidget = await metaRepository.CreateAsync(new Meta
            {
                Key = widgetInfo.Folder,
                Value = JsonConvert.SerializeObject(widget),
                Type = EMetaType.Widget,
            });

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
    }
}

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
        public static WidgetArea BlogSidebar1 = new WidgetArea { Id = "blog-sidebar1", Title = "Blog Sidebar1" };
        public static WidgetArea BlogSidebar2 = new WidgetArea { Id = "blog-sidebar2", Title = "Blog Sidebar2" };
        public static WidgetArea BlogBeforePost = new WidgetArea { Id = "blog-beforepost", Title = "Blog Before Post" };
        public static WidgetArea BlogAfterPost = new WidgetArea { Id = "blog-beforepost", Title = "Blog After Post" };
        public static WidgetArea Footer1 = new WidgetArea { Id = "footer1", Title = "Footer 1" };
        public static WidgetArea Footer2 = new WidgetArea { Id = "footer2", Title = "Footer 2" };
        public static WidgetArea Footer3 = new WidgetArea { Id = "footer3", Title = "Footer 3" };

        public const string WIDGET_INFO_FILE_NAME = "widget.json";
        public string WidgetDirectoryName { get; set; } = "Pages/Widgets";

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

        //private const string CACHE_KEY_WIDGETS_BY_AREA = "{0}-widgets";
        //private TimeSpan Cache_Time_Widgets_By_Area = new TimeSpan(0, 10, 0);

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
        public async Task<WidgetAreaViewModel> GetAreaAsync(string areaId)
        {
            var list = await GetCurrentThemeAreasAsync();
            return list.Single(a => a.Id == areaId);
        }

        /// <summary>
        /// Returns a list of widget areas for the current theme, the areas are filled with
        /// widget instances they contain.
        /// </summary>
        public async Task<IEnumerable<WidgetAreaViewModel>> GetCurrentThemeAreasAsync()
        {
            var coreSettings = await settingService.GetSettingsAsync<CoreSettings>();

            var cacheKey = string.Format(CACHE_KEY_CURRENT_THEME_AREAS, coreSettings.Theme);
            return await distributedCache.GetAsync(cacheKey, Cache_Time_Current_Theme_Areas, async () =>
            {
                var list = new List<WidgetAreaViewModel>();

                var themeInfos = await themeService.GetInstalledThemesInfoAsync();
                var currentTheme = themeInfos.Single(t => t.Name == coreSettings.Theme);
                var areaIds = currentTheme.WidgetAreas; // current theme's widget areas ids

                foreach (var areaId in areaIds)
                {
                    var metaArea = await metaRepository.GetAsync(areaId);
                    var widgetArea = JsonConvert.DeserializeObject<WidgetArea>(metaArea.Value);

                    var widgetAreaViewModel = new WidgetAreaViewModel {
                        Id = widgetArea.Id,
                        Title = widgetArea.Title,
                        WidgetIds = widgetArea.WidgetIds,
                    };

                    foreach (var id in widgetArea.WidgetIds)
                    {
                        var metaWidget = await metaRepository.GetAsync(id);
                        var type = Type.GetType(metaWidget.Key);
                        var widget = (Widget)JsonConvert.DeserializeObject(metaWidget.Value, type);

                        var widgetInfos = await GetInstalledWidgetsInfoAsync();
                        var widgetInfo = widgetInfos.Single(wi => wi.Type == metaWidget.Key);
                        var widgetViewModel = new WidgetViewModel {
                            Id = id, // set id
                            Title = widget.Title,
                            Name = widgetInfo.Name,
                        };
                        widgetAreaViewModel.Widgets.Add(widgetViewModel);
                    }

                    list.Add(widgetAreaViewModel);
                }

                return list;
            });
        }

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
                //var widgetsFolder = Path.Combine(hostingEnvironment.ContentRootPath, WIDGET_DIRECTORY_NAME);
                var widgetsFolder = Path.Combine(hostingEnvironment.ContentRootPath, WidgetDirectoryName);

                foreach (var dir in Directory.GetDirectories(widgetsFolder))
                {
                    var file = Path.Combine(dir, WIDGET_INFO_FILE_NAME);
                    var info = JsonConvert.DeserializeObject<WidgetInfo>(await File.ReadAllTextAsync(file));
                    list.Add(info);
                }

                return list;
            });
        }

        /// <summary>
        /// Creates a widget instance when user drops a widget in a widget area.
        /// </summary>
        /// <param name="widgetType"></param>
        /// <param name="areaId"></param>
        /// <returns>The id of the new instance.</returns>
        /// <remarks>
        /// When a user drops a widget into a widget area, an instance of the widget will be created 
        /// then the area is updated with the new widget instance's id added to its id list.
        /// </remarks>
        public async Task<Widget> AddWidgetAsync(string widgetType, string areaId, int index)
        {
            // create new widget instance
            var type = Type.GetType(widgetType);
            var widget = (Widget)Activator.CreateInstance(type);

            // create widget meta record
            var metaWidget = await metaRepository.CreateAsync(new Meta
            {
                Key = widgetType, // widget type as Key in meta
                Value = JsonConvert.SerializeObject(widget),
                Type = EMetaType.Widget,
            });

            // get area
            var metaArea = await metaRepository.GetAsync(areaId);
            var area = JsonConvert.DeserializeObject<WidgetArea>(metaArea.Value);

            // insert new id to area
            List<int> widgetIdsList = area.WidgetIds.ToList();
            widgetIdsList.Insert(index, metaWidget.Id);
            area.WidgetIds = widgetIdsList.ToArray();

            // update meta
            metaArea.Value = JsonConvert.SerializeObject(area);
            await metaRepository.UpdateAsync(metaArea);

            // invalidate areas from cache
            var coreSettings = await settingService.GetSettingsAsync<CoreSettings>();
            var cacheKey = string.Format(CACHE_KEY_CURRENT_THEME_AREAS, coreSettings.Theme);
            await distributedCache.RemoveAsync(cacheKey);

            // set id
            widget.Id = metaWidget.Id;
            return widget;
        }

        /// <summary>
        /// Returns a widget for update.
        /// </summary>
        /// <param name="widgetType"></param>
        /// <returns></returns>
        public async Task<Widget> GetWidgetAsync(int id)
        {
            var widgetMeta = await metaRepository.GetAsync(id);
            var type = Type.GetType(widgetMeta.Key); // Key is the widget type

            var widget = (Widget)JsonConvert.DeserializeObject(widgetMeta.Value, type);
            widget.Id = id;

            return widget;
        }

        public async Task UpdateWidgetAsync(Widget widget)
        {
            var meta = await metaRepository.GetAsync(widget.Id);
            meta.Value = JsonConvert.SerializeObject(widget);
            await metaRepository.UpdateAsync(meta);
        }

        /// <summary>
        /// Remove the widget instance from area by delete the widget instance meta record and
        /// delete the id from the area mete record's id array.
        /// </summary>
        /// <param name="widgetId"></param>
        /// <param name="areaId"></param>
        /// <returns></returns>
        /// <remarks>
        /// Remove a widget from area will invalidate cache on the areas.
        /// </remarks>
        public async Task RemoveWidgetAsync(int widgetId, string areaId)
        {
            // delete the instance
            await metaRepository.DeleteAsync(widgetId);

            // get the area from db
            var metaArea = await metaRepository.GetAsync(areaId);
            var widgetArea = JsonConvert.DeserializeObject<WidgetArea>(metaArea.Value);

            // delete the id from area's id array
            widgetArea.WidgetIds = widgetArea.WidgetIds.Where(id => id != widgetId).ToArray();

            // update the area
            metaArea.Value = JsonConvert.SerializeObject(widgetArea);
            await metaRepository.UpdateAsync(metaArea);

            // invalidate areas from cache
            var coreSettings = await settingService.GetSettingsAsync<CoreSettings>();
            var cacheKey = string.Format(CACHE_KEY_CURRENT_THEME_AREAS, coreSettings.Theme);
            await distributedCache.RemoveAsync(cacheKey);
        }
    }
}

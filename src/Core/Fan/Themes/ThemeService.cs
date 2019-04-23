using Fan.Data;
using Fan.Exceptions;
using Fan.Settings;
using Fan.Widgets;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static MoreLinq.Extensions.DistinctByExtension;

namespace Fan.Themes
{
    public class ThemeService : IThemeService
    {
        public const string THEME_INFO_FILE_NAME = "theme.json";
        public const string THEME_DIRECTORY_NAME = "Themes";
        /// <summary>
        /// A theme's folder can only contain alphanumeric, dash and underscore.
        /// </summary>
        public const string THEME_FOLDER_REGEX = @"^[a-zA-Z0-9-_]+$";

        private readonly ISettingService settingService;
        private readonly IHostingEnvironment hostingEnvironment;
        private readonly IDistributedCache distributedCache;
        private readonly IMetaRepository metaRepository;
        private readonly ILogger<ThemeService> logger;

        public ThemeService(ISettingService settingService,
            IHostingEnvironment hostingEnvironment,
            IDistributedCache distributedCache,
            IMetaRepository metaRepository,
            ILogger<ThemeService> logger)
        {
            this.settingService = settingService;
            this.hostingEnvironment = hostingEnvironment;
            this.distributedCache = distributedCache;
            this.metaRepository = metaRepository;
            this.logger = logger;
        }

        private const string CACHE_KEY_CURRENT_THEME_AREAS = "{0}-theme-widget-areas";
        private TimeSpan Cache_Time_Current_Theme_Areas = new TimeSpan(0, 10, 0);

        /// <summary>
        /// Activates a theme.
        /// </summary>
        /// <param name="folderName">Theme's folder name.</param>
        /// <returns></returns>
        /// <remarks>
        /// It registers theme and the widget areas used by the theme.
        /// </remarks>
        public async Task ActivateThemeAsync(string folderName)
        {
            // verify folderName 
            if (!IsValidFolderName(folderName))
                throw new FanException($"Theme {folderName} contains invalid characters.");

            // register theme if not exist
            folderName = folderName.ToLower(); // lower case
            if (await metaRepository.GetAsync(folderName, EMetaType.Theme) == null)
            {
                await metaRepository.CreateAsync(new Meta
                {
                    Key = folderName, 
                    Value = "", // empty for now
                    Type = EMetaType.Theme,
                });
            }

            // register theme-defined widget areas
            var installedThemes = await GetInstalledManifestInfosAsync();
            var themeToActivate = installedThemes.Single(t => t.Name.Equals(folderName, StringComparison.OrdinalIgnoreCase));

            // check if there is any empty area ids
            if (themeToActivate.WidgetAreas.Any(a => a.Id.IsNullOrEmpty()))
                throw new FanException("Widget area id cannot be empty.");

            var themeDefinedAreas = themeToActivate.WidgetAreas.Where(ta => !WidgetService.SystemDefinedWidgetAreaInfos.Any(sa => sa.Id == ta.Id));
            foreach (var area in themeDefinedAreas)
            {
                var key = string.Format($"{folderName}-{area.Id}").ToLower();

                // register only if not exist
                if (await metaRepository.GetAsync(key, EMetaType.WidgetAreaByTheme) == null)
                {
                    var widgetArea = new WidgetArea { Id = area.Id };
                    await metaRepository.CreateAsync(new Meta
                    {
                        Key = key,
                        Value = JsonConvert.SerializeObject(widgetArea),
                        Type = EMetaType.WidgetAreaByTheme,
                    });
                }
            }
        }

        /// <summary>
        /// Returns a list of <see cref="ThemeInfo"/> of the installed themes.
        /// </summary>
        /// <remarks>
        /// The ids of the list of <see cref="WidgetAreaInfo"/> are distinct and lower case.
        /// </remarks>
        public async Task<IEnumerable<ThemeInfo>> GetInstalledManifestInfosAsync()
        {
            var list = new List<ThemeInfo>();
            var themesDir = Path.Combine(hostingEnvironment.ContentRootPath, THEME_DIRECTORY_NAME);

            foreach (var dir in Directory.GetDirectories(themesDir))
            {
                var dirTokens = dir.Split(Path.DirectorySeparatorChar);
                var folder = dirTokens[dirTokens.Length - 1];

                // load only valid folder name
                if (!IsValidFolderName(folder)) continue;

                var file = Path.Combine(dir, THEME_INFO_FILE_NAME);
                var themeInfo = JsonConvert.DeserializeObject<ThemeInfo>(await File.ReadAllTextAsync(file));
                themeInfo.Folder = folder;

                // make sure no duplicate areas based on id
                themeInfo.WidgetAreas = themeInfo.WidgetAreas.DistinctBy(a => a.Id).ToArray();
                
                // make sure all area ids are lower case
                foreach (var area in themeInfo.WidgetAreas) area.Id = area.Id.ToLower();

                list.Add(themeInfo);
            }

            return list;
        }

        private bool IsValidFolderName(string folder) => new Regex(THEME_FOLDER_REGEX).IsMatch(folder);
    }
}

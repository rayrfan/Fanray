using Fan.Settings;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Fan.Themes
{
    public class ThemeService : IThemeService
    {
        public const string THEME_INFO_FILE_NAME = "theme.json";
        public const string THEME_DIRECTORY_NAME = "Themes";

        private readonly ISettingService settingService;
        private readonly IHostingEnvironment hostingEnvironment;
        private readonly IDistributedCache distributedCache;
        private readonly ILogger<ThemeService> logger;

        public ThemeService(ISettingService settingService,
            IHostingEnvironment hostingEnvironment,
            IDistributedCache distributedCache,
            ILogger<ThemeService> logger)
        {
            this.settingService = settingService;
            this.hostingEnvironment = hostingEnvironment;
            this.distributedCache = distributedCache;
            this.logger = logger;
        }

        public async Task<List<ThemeInfo>> GetInstalledThemesInfoAsync()
        {
            var list = new List<ThemeInfo>();
            var themesDir = Path.Combine(hostingEnvironment.ContentRootPath, THEME_DIRECTORY_NAME);

            foreach (var dir in Directory.GetDirectories(themesDir))
            {
                var file = Path.Combine(dir, THEME_INFO_FILE_NAME);
                var themeInfo = JsonConvert.DeserializeObject<ThemeInfo>(await File.ReadAllTextAsync(file));

                var dirTokens = dir.Split(Path.DirectorySeparatorChar);
                themeInfo.Folder = dirTokens[dirTokens.Length - 1];
                list.Add(themeInfo);
            }

            return list;
        }
    }
}

using Fan.Settings;
using Fan.Themes;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fan.WebApp.Manage.Admin
{
    public class ThemesModel : PageModel
    {
        private readonly IThemeService themeService;
        private readonly ISettingService settingService;

        public ThemesModel(IThemeService themeService, ISettingService settingService)
        {
            this.themeService = themeService;
            this.settingService = settingService;
        }

        public string ThemesJson { get; private set; }

        public async Task OnGet()
        {
            var themeVms = await GetThemeViewModelsAsync();
            ThemesJson = JsonConvert.SerializeObject(themeVms);
        }

        private async Task<IList<ThemeViewModel>> GetThemeViewModelsAsync()
        {
            var manifests = await themeService.GetManifestsAsync();
            var list = new List<ThemeViewModel>();
            var settings = await settingService.GetSettingsAsync<CoreSettings>();
            var currentTheme = settings.Theme;

            foreach (var manifest in manifests)
            {
                var vm = new ThemeViewModel
                {
                    Name = manifest.Name,
                    Screenshot = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/_content/{manifest.Name}/theme.png",
                    IsActive = manifest.Folder.Equals(currentTheme, StringComparison.OrdinalIgnoreCase),
                };

                list.Add(vm);
            }

            return list;
        }
    }

    class ThemeViewModel
    {
        public string Name { get; set; }
        public string Screenshot { get; set; }
        public bool IsActive { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fan.Plugins;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace Fan.WebApp.Manage.Admin
{
    public class PluginsModel : PageModel
    {
        private readonly IPluginService pluginService;
        public const string DEFAULT_ROW_PER_PAGE_ITEMS = "[10, 20]";

        public PluginsModel(IPluginService pluginService)
        {
            this.pluginService = pluginService;
        }

        public string PluginInfosJson { get; private set; }


        public async Task OnGet()
        {
            var pluginInfos = await pluginService.GetInstalledManifestInfosAsync();
            PluginInfosJson = JsonConvert.SerializeObject(pluginInfos);
        }
    }
}
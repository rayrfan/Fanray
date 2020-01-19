using Fan.Plugins;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Linq;
using System.Threading.Tasks;

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

        public string PluginManifestsJson { get; private set; }

        public async Task OnGetAsync()
        {
            var pluginManifests = (await pluginService.GetManifestsAsync()).ToList().OrderBy(p => p.Name);
            PluginManifestsJson = JsonConvert.SerializeObject(pluginManifests);
        }

        public async Task<IActionResult> OnPostActivateAsync([FromBody]PluginDto dto)
        {
            var plugin = await pluginService.ActivatePluginAsync(dto.Folder);
            return new JsonResult(plugin.Id);
        }

        public async Task<IActionResult> OnPostDeactivateAsync([FromBody]PluginDto dto)
        {
            await pluginService.DeactivatePluginAsync(dto.Id);
            return new JsonResult(true);
        }
    }

    public class PluginDto
    {
        public int Id { get; set; }
        public string Folder { get; set; }
    }
}
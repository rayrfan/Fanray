using Fan.Plugins;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ForkMeRibbon.Manage.Plugins
{
    public class ForkMeRibbonSettingsModel : PageModel
    {
        protected readonly IPluginService pluginService;
        public ForkMeRibbonSettingsModel(IPluginService pluginService)
        {
            this.pluginService = pluginService;
        }

        public string ExtJson { get; set; }
        public string PositionsJson { get; set; }
        public ERibbonPosition Position { get; set; }

        public async Task OnGet(int pluginId)
        {
            var plugin = (ForkMeRibbonPlugin)await pluginService.GetExtensionAsync(pluginId);
            ExtJson = JsonConvert.SerializeObject(plugin);

            var positionList = new List<string>();
            foreach (var display in Enum.GetValues(typeof(ERibbonPosition)))
            {
                positionList.Add(display.ToString());
            }
            PositionsJson = JsonConvert.SerializeObject(positionList);
            Position = plugin.Position;
        }

        public async Task<IActionResult> OnPostAsync([FromBody]ForkMeRibbonPlugin plugin)
        {
            if (ModelState.IsValid)
            {
                await pluginService.UpdatePluginAsync(plugin);
                return new JsonResult("Plugin settings updated.");
            }

            return BadRequest("Failed to update plugin settings.");
        }
    }
}
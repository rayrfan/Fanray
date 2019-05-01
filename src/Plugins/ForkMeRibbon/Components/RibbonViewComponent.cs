using Fan.Plugins;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ForkMeRibbon.Components
{
    public class RibbonViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(Plugin plugin)
        {
            var vm = (ForkMeRibbonPlugin)plugin;
            return View("~/Components/Ribbon.cshtml", vm);
        }
    }
}
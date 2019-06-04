using Fan.Plugins;
using Microsoft.AspNetCore.Mvc;

namespace ForkMeRibbon.Components
{
    public class RibbonViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(Plugin plugin)
        {
            var vm = (ForkMeRibbonPlugin)plugin;
            return View("~/Components/Ribbon.cshtml", vm);
        }
    }

    public class RibbonStylesViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(Plugin plugin)
        {
            return View("~/Components/RibbonStyles.cshtml");
        }
    }
}
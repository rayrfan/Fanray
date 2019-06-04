using Fan.Plugins;
using Microsoft.AspNetCore.Mvc;

namespace Shortcodes.Components
{
    public class ShortcodesScriptsViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(Plugin plugin)
        {
            return View("~/Components/ShortcodesScripts.cshtml");
        }
    }

    public class ShortcodesStylesViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(Plugin plugin)
        {
            return View("~/Components/ShortcodesStyles.cshtml");
        }
    }
}

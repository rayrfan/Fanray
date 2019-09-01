using Fan.Plugins;
using Microsoft.AspNetCore.Mvc;

namespace Editor.md.Components
{
    public class EditorMdScriptsViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(Plugin plugin)
        {
            return View("~/Components/EditorMdScripts.cshtml");
        }
    }

    public class EditorMdStylesViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(Plugin plugin)
        {
            // TODO make styles configurable
            return View("~/Components/EditorMdStyles.cshtml");
        }
    }
}
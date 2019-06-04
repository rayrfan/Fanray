using Fan.Widgets;
using Microsoft.AspNetCore.Mvc;

namespace SocialIcons.Components
{
    public class SocialIconsViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(Widget widget)
        {
            var socialIconsWidget = (SocialIconsWidget)widget;
            return View("~/Components/SocialIcons.cshtml", socialIconsWidget);
        }
    }
}

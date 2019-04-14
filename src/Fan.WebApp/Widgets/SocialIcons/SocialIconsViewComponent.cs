using Fan.Widgets;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Fan.WebApp.Widgets.SocialIcons
{
    public class SocialIconsViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(Widget widget)
        {
            var socialIconsWidget = (SocialIconsWidget)widget;
            return View(WidgetService.GetWidgetViewPath("SocialIcons"), socialIconsWidget);
        }
    }
}

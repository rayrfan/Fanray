using Fan.Widgets;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Fan.Web.Widgets.SocialIcons
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

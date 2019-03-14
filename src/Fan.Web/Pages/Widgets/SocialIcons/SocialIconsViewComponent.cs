using Fan.Widgets;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fan.Web.Pages.Widgets.SocialIcons
{
    public class SocialIconsViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(Widget widget)
        {
            var socialIconsWidget = (SocialIconsWidget)widget;
            return View("~/Pages/Widgets/SocialIcons/SocialIcons.cshtml", socialIconsWidget);
        }
    }
}

using Fan.WebApp.Widgets.SocialIcons;
using Fan.Widgets;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Fan.WebApp.Manage.Widgets
{
    public class SocialIconsEditModel : PageModel
    {
        protected readonly IWidgetService widgetService;
        public SocialIconsEditModel(IWidgetService widgetService)
        {
            this.widgetService = widgetService;
        }

        public string WidgetJson { get; set; }

        public class NewUrl
        {
            public string Url { get; set; }
        }

        public async Task OnGet(int widgetId)
        {
            var widget = (SocialIconsWidget)await widgetService.GetExtensionAsync(widgetId);
            WidgetJson = JsonConvert.SerializeObject(widget);
        }

        public IActionResult OnPostAdd([FromBody]NewUrl newUrl)
        {
            var socialLink = SocialIconsWidget.GetSocialLink(newUrl.Url);
            return new JsonResult(socialLink);
        }

        public async Task<IActionResult> OnPostAsync([FromBody]SocialIconsWidget widget)
        {
            if (ModelState.IsValid)
            {
                await widgetService.UpdateWidgetAsync(widget.Id, widget);
                return new JsonResult(true);
            }

            return BadRequest("Invalid form values submitted.");
        }
    }
}
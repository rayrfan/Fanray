using Fan.Widgets;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Fan.Web.Pages.Widgets.RecentBlogPosts
{
    public class EditModel : PageModel
    {
        protected readonly IWidgetService widgetService;
        public EditModel(IWidgetService widgetService)
        {
            this.widgetService = widgetService;
        }

        public string WidgetJson { get; set; }

        public async Task OnGet(int widgetId)
        {
            var widget = (RecentBlogPostsWidget)await widgetService.GetWidgetAsync(widgetId);
            WidgetJson = JsonConvert.SerializeObject(widget);
        }

        public async Task<IActionResult> OnPostAsync([FromBody]RecentBlogPostsWidget widget)
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
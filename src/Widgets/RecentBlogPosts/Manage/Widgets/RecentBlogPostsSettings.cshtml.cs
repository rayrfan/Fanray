using Fan.Blog.Services.Interfaces;
using Fan.Widgets;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace RecentBlogPosts.Manage.Widgets
{
    public class RecentBlogPostsSettingsModel : PageModel
    {
        protected readonly IWidgetService widgetService;
        private readonly IBlogPostService blogPostService;

        public RecentBlogPostsSettingsModel(IWidgetService widgetService, 
            IBlogPostService blogPostService)
        {
            this.widgetService = widgetService;
            this.blogPostService = blogPostService;
        }

        public string WidgetJson { get; set; }

        public async Task OnGet(int widgetId)
        {
            var widget = (RecentBlogPostsWidget)await widgetService.GetExtensionAsync(widgetId);
            WidgetJson = JsonConvert.SerializeObject(widget);
        }

        public async Task<IActionResult> OnPostAsync([FromBody]RecentBlogPostsWidget widget)
        {
            if (ModelState.IsValid)
            {
                await widgetService.UpdateWidgetAsync(widget.Id, widget);
                await blogPostService.RemoveBlogCacheAsync();
                return new JsonResult("Widget settings updated.");
            }

            return BadRequest("Invalid form values submitted.");
        }
    }
}
using Fan.WebApp.Widgets.BlogCategories;
using Fan.Widgets;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Fan.WebApp.Manage.Widgets
{
    public class BlogCategoriesEditModel : PageModel
    {
        protected readonly IWidgetService widgetService;
        public BlogCategoriesEditModel(IWidgetService widgetService)
        {
            this.widgetService = widgetService;
        }

        public string WidgetJson { get; set; }

        /// <summary>
        /// Initializes model properties.
        /// </summary>
        /// <param name="widgetId"></param>
        public async Task OnGet(int widgetId)
        {
            var widget = (BlogCategoriesWidget)await widgetService.GetExtensionAsync(widgetId);
            WidgetJson = JsonConvert.SerializeObject(widget);
        }

        /// <summary>
        /// Updates widget properties. 
        /// </summary>
        /// <param name="widget"></param>
        public async Task<IActionResult> OnPostAsync([FromBody]BlogCategoriesWidget widget)
        {
            if (ModelState.IsValid)
            {
                await widgetService.UpdateWidgetAsync(widget.Id, widget);
                return new JsonResult("Widget settings updated.");
            }

            return BadRequest("Invalid form values submitted.");
        }
    }
}
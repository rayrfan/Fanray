using Fan.Blog.Models;
using Fan.Blog.Services.Interfaces;
using Fan.Widgets;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fan.Web.Widgets.BlogCategories
{
    /// <summary>
    /// The BlogCategories view component.
    /// </summary>
    public class BlogCategoriesViewComponent : ViewComponent
    {
        private readonly ICategoryService _catSvc;
        public BlogCategoriesViewComponent(ICategoryService catService)
        {
            _catSvc = catService;
        }

        public async Task<IViewComponentResult> InvokeAsync(Widget widget)
        {
            var blogCategoriesWidget = (BlogCategoriesWidget)widget;
            var categories = (await _catSvc.GetAllAsync()).Where(t => t.Count > 0);

            return View(WidgetService.GetWidgetViewPath("BlogCategories"), 
                new Tuple<IEnumerable<Category>, BlogCategoriesWidget>(categories, blogCategoriesWidget));
        }
    }
}
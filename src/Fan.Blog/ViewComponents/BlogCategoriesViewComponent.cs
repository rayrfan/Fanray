using Fan.Blog.Services;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace Fan.Blog.ViewComponents
{
    /// <summary>
    /// The BlogCategories view component.
    /// </summary>
    public class BlogCategoriesViewComponent : ViewComponent
    {
        private readonly IBlogService _blogSvc;
        public BlogCategoriesViewComponent(IBlogService blogService)
        {
            _blogSvc = blogService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var cats = (await _blogSvc.GetCategoriesAsync()).Where(t => t.Count > 0);
            return View(cats);
        }
    }
}
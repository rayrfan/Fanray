using Fan.Blog.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace Fan.Web.ViewComponents
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

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var cats = (await _catSvc.GetAllAsync()).Where(t => t.Count > 0);
            return View(cats);
        }
    }
}
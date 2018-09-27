using Fan.Blog.Services;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace Fan.Blog.ViewComponents
{
    /// <summary>
    /// The BlogTags view component.
    /// </summary>
    public class BlogTagsViewComponent : ViewComponent
    {
        private readonly IBlogService _blogSvc;
        public BlogTagsViewComponent(IBlogService blogService)
        {
            _blogSvc = blogService;
        }

        /// <summary>
        /// Returns the default view for BlogTags. TODO provide param for things like sorting.
        /// </summary>
        /// <returns></returns>
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var tags = (await _blogSvc.GetTagsAsync()).Where(t => t.Count > 0);
            return View(tags);
        }
    }
}
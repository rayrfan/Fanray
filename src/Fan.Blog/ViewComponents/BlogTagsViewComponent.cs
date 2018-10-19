using Fan.Blog.Services;
using Fan.Blog.Tags;
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
        private readonly ITagService _tagSvc;
        public BlogTagsViewComponent(ITagService tagService)
        {
            _tagSvc = tagService;
        }

        /// <summary>
        /// Returns the default view for BlogTags. TODO provide param for things like sorting.
        /// </summary>
        /// <returns></returns>
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var tags = (await _tagSvc.GetTagsAsync()).Where(t => t.Count > 0);
            return View(tags);
        }
    }
}
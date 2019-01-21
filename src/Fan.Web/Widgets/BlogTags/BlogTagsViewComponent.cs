using Fan.Blog.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace Fan.Web.Widgets
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
            var tags = (await _tagSvc.GetAllAsync()).Where(t => t.Count > 0);
            return View("~/Widgets/BlogTags/BlogTags.cshtml", tags);
        }
    }
}
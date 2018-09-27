using Fan.Blog.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Fan.Blog.ViewComponents
{
    /// <summary>
    /// The BlogArchives view component.
    /// </summary>
    public class BlogArchivesViewComponent : ViewComponent
    {
        private readonly IBlogService _blogSvc;
        public BlogArchivesViewComponent(IBlogService blogService)
        {
            _blogSvc = blogService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var years = await _blogSvc.GetArchivesAsync();
            return View(years);
        }
    }
}
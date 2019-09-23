using Fan.Blog.Models.View;
using Fan.Web.Models.Blog;
using System.Threading.Tasks;

namespace Fan.Web.Helpers
{
    public interface IHomeHelper
    {
        Task<(string viewPath, BlogPostListViewModel viewModel)> GetBlogIndexAsync(int? page);
        Task<(string viewPath, BlogPostListViewModel viewModel)> GetBlogCategoryAsync(string slug);
        Task<(string viewPath, PageVM viewModel)> GetPageAsync(string parentPage, string childPage = null);
    }
}

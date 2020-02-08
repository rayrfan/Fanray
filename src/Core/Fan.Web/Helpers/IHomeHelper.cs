using Fan.Blog.Models.View;
using System.Threading.Tasks;

namespace Fan.Web.Helpers
{
    public interface IHomeHelper
    {
        Task<(string viewPath, BlogPostListVM viewModel)> GetBlogIndexAsync(int? page);
        Task<(string viewPath, BlogPostListVM viewModel)> GetBlogCategoryAsync(string slug, int? page);
        Task<(string viewPath, PageVM viewModel)> GetPageAsync(string parentPage, string childPage = null);
    }
}

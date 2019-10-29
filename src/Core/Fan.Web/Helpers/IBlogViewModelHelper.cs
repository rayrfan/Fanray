using Fan.Blog.Models;
using Fan.Blog.Models.View;
using System.Threading.Tasks;

namespace Fan.Web.Helpers
{
    public interface IBlogViewModelHelper
    {
        Task<BlogPostVM> GetBlogPostVMAsync(BlogPost blogPost);
        Task<BlogPostVM> GetBlogPostVMPreviewAsync(BlogPost blogPost);
        Task<BlogPostListVM> GetBlogPostListVMAsync(BlogPostList blogPostList, int currentPage = 1);
        Task<BlogPostListVM> GetBlogPostListVMForArchiveAsync(BlogPostList blogPostList, int? year, int? month);
        Task<BlogPostListVM> GetBlogPostListVMForCategoryAsync(BlogPostList blogPostList, Category cat, int currentPage = 1);
        Task<BlogPostListVM> GetBlogPostListVMForTagAsync(BlogPostList blogPostList, Tag tag);
    }
}

using Fan.Blogs.Models;
using Fan.Blogs.ViewModels;
using System.Threading.Tasks;

namespace Fan.Blogs.Services
{
    public interface IBlogMapper
    {
        Task<BlogPostViewModel> GetBlogPostViewModelAsync(BlogPost post);
        Task<BlogPostViewModelList> GetBlogPostViewModelListAsync(BlogPostList postList);
    }
}

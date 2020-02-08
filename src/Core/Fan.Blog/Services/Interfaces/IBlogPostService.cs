using Fan.Blog.Models;
using System.Threading.Tasks;

namespace Fan.Blog.Services.Interfaces
{
    /// <summary>
    /// The blog post service contract.
    /// </summary>
    public interface IBlogPostService
    {
        /// <summary>
        /// Creates a <see cref="BlogPost"/>.
        /// </summary>
        Task<BlogPost> CreateAsync(BlogPost post);
        /// <summary>
        /// Updates a <see cref="BlogPost"/>.
        /// </summary>
        Task<BlogPost> UpdateAsync(BlogPost post);
        /// <summary>
        /// Deletes a <see cref="BlogPost"/> by id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task DeleteAsync(int id);
        /// <summary>
        /// Returns a <see cref="BlogPost"/> by id, throws <see cref="FanException"/> if not found.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="FanException">if post is not found.</exception>
        Task<BlogPost> GetAsync(int id);
        /// <summary>
        /// Returns a <see cref="BlogPost"/> by slug, year, month and day.
        /// </summary>
        Task<BlogPost> GetAsync(string slug, int year, int month, int day);
        /// <summary>
        /// Returns <see cref="BlogPostList"/> for blog main index.
        /// </summary>
        /// <param name="pageIndex">Pagination 1-based</param>
        Task<BlogPostList> GetListAsync(int pageIndex, int pageSize, bool cacheable = true);
        /// <summary>
        /// Returns <see cref="BlogPostList"/> for a blog category.
        /// </summary>
        /// <param name="categorySlug"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        Task<BlogPostList> GetListForCategoryAsync(string categorySlug, int pageIndex);
        /// <summary>
        /// Returns <see cref="BlogPostList"/> for a blog tag.
        /// </summary>
        /// <param name="tagSlug"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        Task<BlogPostList> GetListForTagAsync(string tagSlug, int pageIndex);
        /// <summary>
        /// Returns <see cref="BlogPostList"/> for archive.
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        Task<BlogPostList> GetListForArchive(int? year, int? month, int page = 1);
        /// <summary>
        /// Returns all blog post drafts.
        /// </summary>
        /// <returns></returns>
        Task<BlogPostList> GetListForDraftsAsync();
        /// <summary>
        /// Returns specific number of <see cref="BlogPost"/> used by metaweblog.
        /// </summary>
        /// <param name="numberOfPosts">"All" is int.MaxValue</param>
        /// <returns></returns>
        Task<BlogPostList> GetRecentPostsAsync(int numberOfPosts);
        /// <summary>
        /// Returns spcific number of published <see cref="BlogPost"/>.
        /// </summary>
        /// <param name="numberOfPosts"></param>
        /// <returns></returns>
        Task<BlogPostList> GetRecentPublishedPostsAsync(int numberOfPosts);
        /// <summary>
        /// Invalidates blog cache.
        /// </summary>
        /// <returns></returns>
        Task RemoveBlogCacheAsync();
    }
}
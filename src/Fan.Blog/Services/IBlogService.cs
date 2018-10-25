using Fan.Blog.Enums;
using Fan.Blog.Models;
using Fan.Medias;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Fan.Blog.Services
{
    /// <summary>
    /// The blog service contract.
    /// </summary>
    public interface IBlogService
    {
        // -------------------------------------------------------------------- Archive 

        /// <summary>
        /// Returns a dictionary of year and months, the key is year and the value is a list of 
        /// <see cref="MonthItem"/> objects.
        /// </summary>
        /// <returns></returns>
        Task<Dictionary<int, List<MonthItem>>> GetArchivesAsync();

        // -------------------------------------------------------------------- Images

        /// <summary>
        /// Deletes an image by id.
        /// </summary>
        /// <param name="mediaId"></param>
        /// <returns></returns>
        Task DeleteImageAsync(int mediaId);

        /// <summary>
        /// Returns absolute URL to an image.
        /// </summary>
        /// <param name="media">The media record representing the image.</param>
        /// <param name="size">The image size.</param>
        /// <returns></returns>
        string GetImageUrl(Media media, EImageSize size);

        /// <summary>
        /// Uploads image.
        /// </summary>
        /// <param name="source">File stream.</param>
        /// <param name="userId">User who uploads the file.</param>
        /// <param name="fileName">Original filename.</param>
        /// <param name="contentType">File content type e.g. "image/jpeg".</param>
        /// <param name="uploadFrom">Whether the image is uploaded from browser or OLW.</param>
        /// <returns>
        /// <see cref="Media"/> that represents the image.
        /// </returns>
        Task<Media> UploadImageAsync(Stream source, int userId, string fileName, string contentType, EUploadedFrom uploadFrom);

        // -------------------------------------------------------------------- Posts

        /// <summary>
        /// Creates a <see cref="BlogPost"/>.
        /// </summary>
        Task<BlogPost> CreatePostAsync(BlogPost post);
        /// <summary>
        /// Updates a <see cref="BlogPost"/>.
        /// </summary>
        Task<BlogPost> UpdatePostAsync(BlogPost post);
        /// <summary>
        /// Deletes a <see cref="BlogPost"/> by id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task DeletePostAsync(int id);
        /// <summary>
        /// Returns a <see cref="BlogPost"/> by id, throws <see cref="FanException"/> if not found.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="FanException">if post is not found.</exception>
        Task<BlogPost> GetPostAsync(int id);
        /// <summary>
        /// Returns a <see cref="BlogPost"/> by slug, year, month and day.
        /// </summary>
        Task<BlogPost> GetPostAsync(string slug, int year, int month, int day);
        /// <summary>
        /// Returns <see cref="BlogPostList"/> for blog main index.
        /// </summary>
        /// <param name="pageIndex">Pagination 1-based</param>
        Task<BlogPostList> GetPostsAsync(int pageIndex, int pageSize);
        /// <summary>
        /// Returns <see cref="BlogPostList"/> for a blog category.
        /// </summary>
        /// <param name="categorySlug"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        Task<BlogPostList> GetPostsForCategoryAsync(string categorySlug, int pageIndex);
        /// <summary>
        /// Returns <see cref="BlogPostList"/> for a blog tag.
        /// </summary>
        /// <param name="tagSlug"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        Task<BlogPostList> GetPostsForTagAsync(string tagSlug, int pageIndex);
        /// <summary>
        /// Returns <see cref="BlogPostList"/> for archive.
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        Task<BlogPostList> GetPostsForArchive(int? year, int? month, int page = 1);
        /// <summary>
        /// Returns all blog post drafts.
        /// </summary>
        /// <returns></returns>
        Task<BlogPostList> GetPostsForDraftsAsync();
        /// <summary>
        /// Returns specified number of <see cref="BlogPost"/> used by metaweblog.
        /// </summary>
        /// <param name="numberOfPosts">"All" is int.MaxValue</param>
        /// <returns></returns>
        Task<BlogPostList> GetRecentPostsAsync(int numberOfPosts);

        // -------------------------------------------------------------------- Stats

        /// <summary>
        /// Returns total number of posts by each <see cref="EPostStatus"/>.
        /// </summary>
        /// <returns></returns>
        Task<PostCount> GetPostCountAsync();
    }
}
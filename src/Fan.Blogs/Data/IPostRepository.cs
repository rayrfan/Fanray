using Fan.Blogs.Enums;
using Fan.Blogs.Models;
using Fan.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fan.Blogs.Data
{
    /// <summary>
    /// Contract for a post repository.
    /// </summary>
    public interface IPostRepository : IRepository<Post>
    {
        /// <summary>
        /// Deletes a <see cref="Post"/> by Id, if the post is a root page, 
        /// it will also delete all child pages.
        /// </summary>
        Task DeleteAsync(int id);

        /// <summary>
        /// Returns a <see cref="Post"/> by id. If it is a BlogPost it'll return together with its 
        /// <see cref="Category"/> and <see cref="Tag"/>. Returns null if it's not found.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type">If it's BlogPost it'll return category and tags with it.</param>
        Task<Post> GetAsync(int id, EPostType type);

        /// <summary>
        /// Returns a <see cref="Post"/> by slug. If it is a BlogPost it'll return together with its 
        /// <see cref="Category"/> and <see cref="Tag"/>. Returns null if it's not found.
        /// </summary>
        /// <param name="slug"></param>
        /// <param name="type">If it's BlogPost it'll return category and tags with it.</param>
        /// <returns></returns>
        Task<Post> GetAsync(string slug, EPostType type);

        /// <summary>
        /// Returns a <see cref="EPostStatus.Published"/> <see cref="Post"/>, returns null if it's not found.
        /// </summary>
        Task<Post> GetAsync(string slug, int year, int month, int day);

        /// <summary>
        /// Returns a list of posts and total post count by query or empty list if no posts found.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        Task<(List<Post> posts, int totalCount)> GetListAsync(PostListQuery query);

        /// <summary>
        /// Returns CreatedOn of all published blog posts, used for archives.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<DateTime>> GetPostDateTimesAsync();

        /// <summary>
        /// Returns total number of posts by each <see cref="EPostStatus"/>.
        /// </summary>
        /// <returns></returns>
        Task<PostCount> GetPostCountAsync();
    }
}
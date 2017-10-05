using Fan.Enums;
using Fan.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fan.Data
{
    /// <summary>
    /// Contract for a post repository.
    /// </summary>
    public interface IPostRepository
    {
        /// <summary>
        /// Creates a new <see cref="Post"/>
        /// </summary>
        Task<Post> CreateAsync(Post post);

        /// <summary>
        /// Deletes a <see cref="Post"/> by Id, if the post is a root page, 
        /// it will also delete all child pages.
        /// </summary>
        Task DeleteAsync(int id);

        /// <summary>
        /// Updates a <see cref="Post"/>.
        /// </summary>
        /// <param name="post">Not all implementations use this parameter, such as the Sql ones.</param>
        /// <returns></returns>
        Task<Post> UpdateAsync(Post post);

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
    }
}
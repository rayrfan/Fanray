using Fan.Blog.Models;
using Fan.Exceptions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fan.Blog.Services.Interfaces
{
    /// <summary>
    /// The tag service contract.
    /// </summary>
    public interface ITagService
    {
        /// <summary>
        /// Returns tag by id, throws <see cref="FanException"/> if tag with id is not found.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<Tag> GetAsync(int id);
        /// <summary>
        /// Returns tag by slug, throws <see cref="FanException"/> if tag with slug is not found.
        /// </summary>
        /// <param name="slug"></param>
        /// <returns></returns>
        Task<Tag> GetBySlugAsync(string slug);
        /// <summary>
        /// Returns tag by title, throws <see cref="FanException"/> if tag with title is not found.
        /// </summary>
        /// <param name="title">Tag title.</param>
        /// <returns></returns>
        Task<Tag> GetByTitleAsync(string title);
        /// <summary>
        /// Returns all tags, cached after calls to DAL.
        /// </summary>
        /// <returns></returns>
        Task<List<Tag>> GetAllAsync();
        /// <summary>
        /// Creates a new <see cref="Tag"/>.
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        Task<Tag> CreateAsync(Tag tag);
        /// <summary>
        /// Updates an existing <see cref="Tag"/>.
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        Task<Tag> UpdateAsync(Tag tag);
        /// <summary>
        /// Deletes a <see cref="Tag"/>.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task DeleteAsync(int id);
    }
}

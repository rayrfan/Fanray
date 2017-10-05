using Fan.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fan.Data
{
    /// <summary>
    /// Contract for a tag repository.
    /// </summary>
    public interface ITagRepository
    {
        /// <summary>
        /// Creates a <see cref="Tag"/>.
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        Task<Tag> CreateAsync(Tag tag);

        /// <summary>
        /// Deletes a tag and its associated posts.
        /// </summary>
        Task DeleteAsync(int id);

        /// <summary>
        /// Returns all tags or empty list if no tags found.
        /// </summary>
        Task<List<Tag>> GetListAsync();

        /// <summary>
        /// Updates a <see cref="Tag"/>.
        /// </summary>
        /// <param name="tag">Not all implementations use this parameter, such as the Sql ones.</param>
        Task<Tag> UpdateAsync(Tag tag);
    }
}
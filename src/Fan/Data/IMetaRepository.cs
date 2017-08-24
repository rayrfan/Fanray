using Fan.Models;
using System.Threading.Tasks;

namespace Fan.Data
{
    /// <summary>
    /// Contract for a meta repository.
    /// </summary>
    public interface IMetaRepository
    {
        /// <summary>
        /// Creates a <see cref="Meta"/>.
        /// </summary>
        /// <param name="meta"></param>
        /// <returns></returns>
        Task<Meta> CreateAsync(Meta meta);

        /// <summary>
        /// Returns a <see cref="Meta"/> by its key, returns null if it's not found.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<Meta> GetAsync(string key);

        /// <summary>
        /// Updates a <see cref="Meta"/>.
        /// </summary>
        /// <returns></returns>
        /// <param name="meta">Not all implementations use this parameter, such as the Sql ones.</param>
        Task<Meta> UpdateAsync(Meta meta);
    }
}
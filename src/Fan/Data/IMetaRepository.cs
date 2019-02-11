using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fan.Data
{
    /// <summary>
    /// Contract for a meta repository.
    /// </summary>
    public interface IMetaRepository : IRepository<Meta>
    {
        /// <summary>
        /// Returns a <see cref="Meta"/> by its key, returns null if it's not found.
        /// </summary>
        /// <param name="key">The key must be in proper case.</param>
        /// <returns></returns>
        Task<Meta> GetAsync(string key);
    }
}
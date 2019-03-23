using System.Threading.Tasks;

namespace Fan.Data
{
    /// <summary>
    /// Contract for a meta repository.
    /// </summary>
    public interface IMetaRepository : IRepository<Meta>
    {
        /// <summary>
        /// Returns a <see cref="Meta"/> by its key (case-insensitive) and <see cref="EMetaType"/>, returns null if it's not found.
        /// </summary>
        /// <param name="key">The key's casing is ignored.</param>
        /// <param name="type">The <see cref="EMetaType"/> of the meta.</param>
        /// <returns></returns>
        /// <remarks>
        /// A meta record is unique by combination of key and type.
        /// </remarks>
        Task<Meta> GetAsync(string key, EMetaType type);
    }
}
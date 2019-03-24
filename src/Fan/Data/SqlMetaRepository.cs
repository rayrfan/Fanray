using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Fan.Data
{
    /// <summary>
    /// Sql implementation of the <see cref="IMetaRepository"/> contract.
    /// </summary>
    public class SqlMetaRepository : EntityRepository<Meta>, IMetaRepository
    {
        private readonly FanDbContext _db;

        public SqlMetaRepository(FanDbContext db) : base(db)
        {
            _db = db;
        }

        /// <summary>
        /// Returns a <see cref="Meta"/> by its key (case-insensitive) and <see cref="EMetaType"/>, returns null if it's not found.
        /// </summary>
        /// <param name="key">The key's casing is ignored.</param>
        /// <param name="type">The <see cref="EMetaType"/> of the meta.</param>
        /// <returns></returns>
        /// <remarks>
        /// A meta record is unique by combination of key and type.
        /// </remarks>
        public async Task<Meta> GetAsync(string key, EMetaType type) =>
             await _entities.SingleOrDefaultAsync(m => m.Key.Equals(key, StringComparison.OrdinalIgnoreCase)
                                                    && m.Type == type);
    }
}
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
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
        /// Returns all the meta records, returns empty list if no records are found.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// TODO I should probably limit this instead of return all.
        /// </remarks>
        public async Task<List<Meta>> AllAsync() => await _entities.AsNoTracking().ToListAsync();

        /// <summary>
        /// Returns a <see cref="Meta"/> by its key, returns null if it's not found.
        /// </summary>
        /// <param name="key">The caller should pass this key in proper casing.</param>
        /// <returns></returns>
        public async Task<Meta> GetAsync(string key) =>
             await _entities.SingleOrDefaultAsync(m => m.Key == key);
    }
}
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
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
        /// Returns a <see cref="Meta"/> by its key (case-sensitive) and type, returns null if it's not found.
        /// </summary>
        /// <param name="key">The caller should pass this key in proper casing.</param>
        /// <param name="type">The <see cref="EMetaType"/> of the meta.</param>
        /// <returns></returns>
        /// <remarks>
        /// A meta record is unique by combination of key and type.
        /// </remarks>
        public async Task<Meta> GetAsync(string key, EMetaType type) =>
            await _entities.SingleOrDefaultAsync(m => m.Key == key && m.Type == type);

        public async Task<List<Meta>> GetListAsync(EMetaType type) =>
            await _entities.Where(m => m.Type == type).ToListAsync();
    }
}
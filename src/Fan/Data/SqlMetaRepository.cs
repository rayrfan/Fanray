using Fan.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Fan.Data
{
    /// <summary>
    /// Sql implementation of the <see cref="IMetaRepository"/> contract.
    /// </summary>
    public class SqlMetaRepository : IMetaRepository
    {
        private readonly FanDbContext _db;

        public SqlMetaRepository(FanDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Creates a <see cref="Meta"/> record, throws <see cref="DbUpdateException"/> if there is 
        /// already a record with the same key.  <see cref="Meta.Key"/> has a unique constraint.
        /// </summary>
        /// <param name="meta"></param>
        public async Task<Meta> CreateAsync(Meta meta)
        {
            await _db.AddAsync(meta);
            await _db.SaveChangesAsync();
            return meta;
        }

        /// <summary>
        /// Returns a <see cref="Meta"/> by its key, returns null if it's not found.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<Meta> GetAsync(string key) =>
             await _db.Metas.SingleOrDefaultAsync(m => m.Key.Equals(key, StringComparison.CurrentCultureIgnoreCase));

        /// <summary>
        /// Updates a <see cref="Meta"/>.
        /// </summary>
        /// <param name="meta">Not used just being returned.</param>
        /// <returns></returns>
        public async Task<Meta> UpdateAsync(Meta meta)
        {
            await _db.SaveChangesAsync();
            return meta;
        }
    }
}

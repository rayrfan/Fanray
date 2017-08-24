using System;
using System.Threading.Tasks;
using Fan.Models;
using Fan.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Fan.Data
{
    public class SqlMetaRepository : IMetaRepository
    {
        private readonly FanDbContext _db;

        public SqlMetaRepository(FanDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Creates a <see cref="Meta"/>.
        /// </summary>
        /// <param name="meta"></param>
        /// <exception cref="FanException">If key already exists.</exception>
        public async Task<Meta> CreateAsync(Meta meta)
        {
            if (await this.GetAsync(meta.Key) != null)
                throw new FanException("Meta record already exists in database.");

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

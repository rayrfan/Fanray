using Fan.Enums;
using Fan.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fan.Data
{
    /// <summary>
    /// Sql implementation of the <see cref="IMetaRepository"/> contract.
    /// </summary>
    public class SqlMetaRepository : EFRepository<Meta>, IMetaRepository
    {
        private readonly CoreDbContext _db;

        public SqlMetaRepository(CoreDbContext db) : base(db)
        {
            _db = db;
        }

        /// <summary>
        /// Returns a <see cref="Meta"/> by its key, returns null if it's not found.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<Meta> GetAsync(string key) =>
             await _db.Metas.SingleOrDefaultAsync(m => m.Key.Equals(key, StringComparison.CurrentCultureIgnoreCase));

        /// <summary>
        /// Returns a list of <see cref="Meta"/> records.
        /// </summary>
        /// <param name="keySegment"></param>
        /// <param name="compareBy"></param>
        /// <returns></returns>
        public async Task<List<Meta>> GetListAsync(string keySegment, EMetaKeyCompareBy compareBy)
        {
            switch (compareBy)
            {
                case EMetaKeyCompareBy.StartsWith:
                    return await _db.Metas.Where(m => m.Key.StartsWith(keySegment)).ToListAsync();

                case EMetaKeyCompareBy.EndsWith:
                    return await _db.Metas.Where(m => m.Key.EndsWith(keySegment)).ToListAsync();

                default:
                    return await _db.Metas.Where(m => m.Key.Contains(keySegment)).ToListAsync();
            }
        }
    }
}

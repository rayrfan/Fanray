using Fan.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fan.Settings
{
    public class SqlSettingRepository : EntityRepository<Setting>, ISettingRepository
    {
        private readonly FanDbContext _db;

        public SqlSettingRepository(FanDbContext db) : base(db)
        {
            _db = db;
        }

        /// <summary>
        /// Returns all the settings, returns an emtpy list if no settings are found.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Settings returned are for read only purpose, thus no tracking.
        /// </remarks>
        public async Task<List<Setting>> GetAllSettingsAsync() => await _entities.AsNoTracking().ToListAsync();

        /// <summary>
        /// Returns a <see cref="Meta"/> by its key, returns null if it's not found.
        /// </summary>
        /// <param name="key">The caller should pass this key in proper casing.</param>
        /// <returns></returns>
        public async Task<Setting> GetAsync(string key) =>
             await _entities.SingleOrDefaultAsync(m => m.Key == key);
    }
}

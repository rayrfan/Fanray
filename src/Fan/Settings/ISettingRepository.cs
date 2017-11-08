using Fan.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fan.Settings
{
    public interface ISettingRepository : IRepository<Setting>
    {
        /// <summary>
        /// Returns a <see cref="Setting"/> by its key, returns null if it's not found.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<Setting> GetAsync(string key);
        /// <summary>
        /// Returns a list of all <see cref="Setting"/>.
        /// </summary>
        /// <returns></returns>
        Task<List<Setting>> GetAllSettingsAsync();
    }
}

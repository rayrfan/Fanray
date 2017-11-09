using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fan.Settings
{
    public interface ISettingService
    {
        Task<T> UpsertSettingsAsync<T>(T settings) where T : class, ISettings, new();
        Task<T> GetSettingsAsync<T>() where T : class, ISettings, new();
        Task<List<Setting>> GetAllSettingsAsync();
        Task<bool> SettingsExist();
    }
}

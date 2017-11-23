using System.Threading.Tasks;

namespace Fan.Settings
{
    /// <summary>
    /// The settings service contract.
    /// </summary>
    public interface ISettingService
    {
        Task<T> UpsertSettingsAsync<T>(T settings) where T : class, ISettings, new();
        Task<T> GetSettingsAsync<T>() where T : class, ISettings, new();
        Task<bool> SettingsExist();
    }
}

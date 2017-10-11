using System.Threading.Tasks;

namespace Fan.Services
{
    public interface ISettingService
    {
        Task<T> CreateSettingsAsync<T>(T obj) where T : class, new();
        Task<T> GetSettingsAsync<T>(bool createIfNotExist = false) where T : class, new(); 
        Task<T> UpdateSettingsAsync<T>(T obj) where T : class, new();
    }
}

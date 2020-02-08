using System.Threading.Tasks;

namespace Fan.Settings
{
    /// <summary>
    /// The settings service contract.
    /// </summary>
    public interface ISettingService
    {
        /// <summary>
        /// Returns a type of <see cref="ISettings"/>.
        /// </summary>
        /// <typeparam name="T">The derived <see cref="ISettings"/> type.</typeparam>
        /// <returns></returns>
        Task<T> GetSettingsAsync<T>() where T : class, ISettings, new();
        /// <summary>
        /// Creates or updates a Settings, if a particular setting exists then updates it, else inserts it.
        /// </summary>
        /// <typeparam name="T">The derived <see cref="ISettings"/> type.</typeparam>
        /// <param name="settings"></param>
        /// <returns></returns>
        Task<T> UpsertSettingsAsync<T>(T settings) where T : class, ISettings, new();
    }
}

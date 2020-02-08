using Fan.Data;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Fan.Settings
{
    /// <summary>
    /// The service that manages settings.
    /// </summary>
    public class SettingService : ISettingService
    {
        private readonly IMetaRepository metaRepository;
        private readonly IDistributedCache cache;
        private readonly ILogger<SettingService> logger;

        /// <summary>
        /// Cache key for all settings records.
        /// </summary>
        private const string KEY_ALL_SETTINGS = "All-Settings";
        /// <summary>
        /// Cache time for all setting records is 2 min.
        /// </summary>
        private static readonly TimeSpan Time_All_Settings = new TimeSpan(0, 2, 0);
        /// <summary>
        /// Cache time for each type of settings, e.g. Core, Blog etc. is 30 min.
        /// </summary>
        private static readonly TimeSpan Time_Settings = new TimeSpan(0, 30, 0);

        public SettingService(IMetaRepository metaRepository,
                              IDistributedCache cache,
                              ILogger<SettingService> logger)
        {
            this.metaRepository = metaRepository;
            this.cache = cache;
            this.logger = logger;
        }

        /// <summary>
        /// Returns a type of <see cref="ISettings"/>.
        /// </summary>
        /// <typeparam name="T">The derived <see cref="ISettings"/> type.</typeparam>
        /// <returns></returns>
        public async Task<T> GetSettingsAsync<T>() where T : class, ISettings, new()
        {
            string cacheKey = typeof(T).Name;
            return await cache.GetAsync<T>(cacheKey, Time_Settings, async () =>
            {
                var type = typeof(T);
                var settings = Activator.CreateInstance(type);
                
                var allSettings = await GetAllSettingsAsync();
                foreach (var property in type.GetProperties())
                {
                    if (!property.CanRead || !property.CanWrite)
                        continue;

                    var key = (type.Name + "." + property.Name).ToLowerInvariant(); 
                    var setting = allSettings.SingleOrDefault(s => s.Key == key);
                    if (setting == null)
                        continue;

                    var value = TypeDescriptor.GetConverter(property.PropertyType).CanConvertFrom(typeof(string)) ?
                                TypeDescriptor.GetConverter(property.PropertyType).ConvertFromInvariantString(setting.Value) :
                                JsonConvert.DeserializeObject(setting.Value, property.PropertyType);

                    property.SetValue(settings, value);
                }

                return settings as T;
            });
        }

        /// <summary>
        /// Creates or updates a Settings, if a particular setting exists then updates it, else inserts it.
        /// </summary>
        /// <typeparam name="T">The derived <see cref="ISettings"/> type.</typeparam>
        /// <param name="settings"></param>
        /// <returns></returns>
        public async Task<T> UpsertSettingsAsync<T>(T settings) where T : class, ISettings, new()
        {
            var settingsCreate = new List<Meta>();
            var settingsUpdate = new List<Meta>();
            var allSettings = await GetAllSettingsAsync();

            foreach (var property in typeof(T).GetProperties())
            {
                if (!property.CanRead || !property.CanWrite)
                    continue;

                var value = property.GetValue(settings);
                var valueStr = TypeDescriptor.GetConverter(property.PropertyType).CanConvertFrom(typeof(string)) ?
                               TypeDescriptor.GetConverter(typeof(object)).ConvertToInvariantString(value) :
                               JsonConvert.SerializeObject(value);

                var key = (typeof(T).Name + "." + property.Name).ToLowerInvariant();
                if (allSettings == null || !allSettings.Any(s => s.Key == key))
                {
                    settingsCreate.Add(new Meta
                    {
                        Key = key,
                        Value = valueStr
                    });
                }
                else
                {
                    var setting = await metaRepository.GetAsync(key, EMetaType.Setting);
                    if (setting != null && setting.Value != valueStr)
                    {
                        setting.Value = valueStr;
                        settingsUpdate.Add(setting);
                    }
                }
            }

            if (settingsCreate.Count > 0) await metaRepository.CreateRangeAsync(settingsCreate);
            if (settingsUpdate.Count > 0) await metaRepository.UpdateAsync(settingsUpdate);

            string cacheKey = typeof(T).Name;
            await cache.RemoveAsync(cacheKey);
            await cache.RemoveAsync(KEY_ALL_SETTINGS);
            return settings;
        }

        /// <summary>
        /// Returns and caches all settings, returns null if no setting records found.
        /// </summary>
        /// <remarks>
        /// I cache all settings so that when retrieving each type of settings, e.g. Core, Blog etc. 
        /// will not hit database again.
        /// </remarks>
        private async Task<IEnumerable<Meta>> GetAllSettingsAsync()
        {
            return await cache.GetAsync(KEY_ALL_SETTINGS, Time_All_Settings, async () =>
            {
                return await metaRepository.FindAsync(m => m.Type == EMetaType.Setting);
            });
        }
    }
}

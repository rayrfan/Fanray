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
    public class SettingService : ISettingService
    {
        private readonly ISettingRepository _repo;
        private readonly IDistributedCache _cache;
        private readonly ILogger<SettingService> _logger;
        private const string CACHE_KEY_ALL_SETTINGS = "All-Settings";

        public SettingService(ISettingRepository settingRepo,
                              IDistributedCache cache,
                              ILogger<SettingService> logger)
        {
            _repo = settingRepo;
            _cache = cache;
            _logger = logger;
        }

        /// <summary>
        /// Returns all settings, cached for 10 min, returns null if no setting found.
        /// </summary>
        /// <returns></returns>
        public async Task<List<Setting>> GetAllSettingsAsync()
        {
            return await _cache.GetAsync(CACHE_KEY_ALL_SETTINGS, new TimeSpan(0, 10, 0), async () =>
            {
                return await _repo.GetAllSettingsAsync();
            });
        }

        /// <summary>
        /// Returns a type of <see cref="ISettings"/>, cached for 30 min.
        /// </summary>
        /// <typeparam name="T">The derived <see cref="ISettings"/> type.</typeparam>
        /// <returns></returns>
        public async Task<T> GetSettingsAsync<T>() where T : class, ISettings, new()
        {
            string cacheKey = typeof(T).Name;
            return await _cache.GetAsync<T>(cacheKey, new TimeSpan(0, 30, 0), async () =>
            {
                var type = typeof(T);
                var allSettings = await GetAllSettingsAsync();
                var settings = Activator.CreateInstance(type);
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
        /// Invalidates cache for the Settings when done.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="settings"></param>
        /// <returns></returns>
        public async Task<T> UpsertSettingsAsync<T>(T settings) where T : class, ISettings, new()
        {
            var settingsCreate = new List<Setting>();
            var settingsUpdate = new List<Setting>();
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
                    settingsCreate.Add(new Setting
                    {
                        Key = key,
                        Value = valueStr
                    });
                }
                else
                {
                    var setting = await _repo.GetAsync(key);
                    if (setting != null && setting.Value != valueStr)
                    {
                        setting.Value = valueStr;
                        settingsUpdate.Add(setting);
                    }
                }
            }

            if (settingsCreate.Count > 0) await _repo.CreateRangeAsync(settingsCreate);
            if (settingsUpdate.Count > 0) await _repo.UpdateAsync();

            string cacheKey = typeof(T).Name;
            await _cache.RemoveAsync(cacheKey);
            await _cache.RemoveAsync(CACHE_KEY_ALL_SETTINGS);
            return settings;
        }

        public async Task<bool> SettingsExist()
        {
            var allSettings = await GetAllSettingsAsync();
            return allSettings.Count > 0;
        }
    }
}

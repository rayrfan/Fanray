using Fan.Data;
using Fan.Exceptions;
using Fan.Models;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Fan.Services
{
    public class SettingService : ISettingService
    {
        private readonly IMetaRepository _metaRepo;
        private readonly IDistributedCache _cache;
        private readonly ILogger<SettingService> _logger;

        public SettingService(IMetaRepository metaRepo,
                              IDistributedCache cache,
                              ILogger<SettingService> logger)
        {
            _metaRepo = metaRepo;
            _cache = cache;
            _logger = logger;
        }

        /// <summary>
        /// Creates settings throws <see cref="FanException"/> if a settings of the type already exists.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public async Task<T> CreateSettingsAsync<T>(T obj) where T : class, new()
        {
            string key = typeof(T).Name;
            if (await GetSettingsAsync<T>() != null)
            {
                throw new FanException($"An object of this type {key} already exists.");
            }

            var meta = new Meta
            {
                Key = key,
                Value = JsonConvert.SerializeObject(obj),
            };

            await _metaRepo.CreateAsync(meta);

            return obj;
        }

        /// <summary>
        /// Returns a settings object or null if it does not exist. Optionally caller can ask
        /// this method to create the settings if not exist.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="createIfNotExist"></param>
        /// <returns></returns>
        public async Task<T> GetSettingsAsync<T>(bool createIfNotExist = false) where T : class, new()
        {
            string key = typeof(T).Name;
            return await _cache.GetAsync<T>(key, new TimeSpan(0, 10, 0), async () =>
            {
                var meta = await _metaRepo.GetAsync(key);

                if (meta == null)
                {
                    if (createIfNotExist)
                        return await CreateSettingsAsync(new T());

                    return default(T);
                }

                return JsonConvert.DeserializeObject<T>(meta.Value);
            });

        }

        /// <summary>
        /// Updates a settings object, if settings does not exist it will throw <see cref="FanException"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public async Task<T> UpdateSettingsAsync<T>(T obj) where T : class, new()
        {
            string key = typeof(T).Name;
            var meta = await _metaRepo.GetAsync(key);
            if (meta == null)
            {
                throw new FanException($"{key} cannot be updated because it does not exist.");
            }

            meta.Value = JsonConvert.SerializeObject(obj);

            await _metaRepo.UpdateAsync(meta);
            await _cache.RemoveAsync(key);

            return obj;
        }
    }
}

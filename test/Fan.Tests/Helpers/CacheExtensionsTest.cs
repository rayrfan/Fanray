using Fan.Models;
using Fan.Settings;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Fan.Tests.Helpers
{
    /// <summary>
    /// Test for <see cref="CacheExtensions"/> class.
    /// </summary>
    public class CacheExtensionsTest
    {
        IDistributedCache _cache;

        /// <summary>
        /// Sets up the distributed cache.
        /// </summary>
        public CacheExtensionsTest()
        {
            var serviceProvider = new ServiceCollection().AddMemoryCache().BuildServiceProvider();
            var memCacheOptions = serviceProvider.GetService<IOptions<MemoryDistributedCacheOptions>>();
            _cache = new MemoryDistributedCache(memCacheOptions);
        }

        /// <summary>
        /// This test proves GetAsync is able to cache the object the first time you need it
        /// and subsequently it returns the object from cache without calling the service.
        /// </summary>
        [Fact]
        public async void GetAsync_ExtensionMethod_IsAbleTo_Cache_Object()
        {
            // Arrange: Given a service that returns CoreSettings
            var _svc = new Mock<ISettingService>();
            _svc.Setup(t => t.GetSettingsAsync<CoreSettings>()).Returns(Task.FromResult(new CoreSettings()));

            // Act: When we call the cache for it for the first time, 
            // it calls the method, gets the object and caches it.
            var res1 = await _cache.GetAsync("cache-key", new TimeSpan(0, 10, 0), async () =>
            {
                return await _svc.Object.GetSettingsAsync<CoreSettings>();
            });

            // When we call the cache for it for the second time, 
            // it returns the object from cache and won't call the method.
            var res2 = await _cache.GetAsync("cache-key", new TimeSpan(0, 10, 0), async () =>
            {
                return await _svc.Object.GetSettingsAsync<CoreSettings>();
            });

            // Assert: Then the method has only been called exactly once
            // And the objects returned each time should match in their values
            _svc.Verify(service => service.GetSettingsAsync<CoreSettings>(), Times.Exactly(1));
            Assert.Equal(res1.Title, res2.Title);
        }
    }
}
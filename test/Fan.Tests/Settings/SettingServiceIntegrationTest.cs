using Fan.Models;
using Fan.Settings;
using Fan.Tests.Data;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;

namespace Fan.Tests.Settings
{
    /// <summary>
    /// Integration tests for <see cref="SettingService"/>.
    /// </summary>
    public class SettingServiceIntegrationTest : DataTestBase
    {
        private ISettingService _svc;
        public SettingServiceIntegrationTest()
        {
            var repo = new SqlSettingRepository(_db);
            var serviceProvider = new ServiceCollection().AddMemoryCache().AddLogging().BuildServiceProvider();
            var memCacheOptions = serviceProvider.GetService<IOptions<MemoryDistributedCacheOptions>>();
            var cache = new MemoryDistributedCache(memCacheOptions);
            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger<SettingService>();
            _svc = new SettingService(repo, cache, logger);
        }

        [Fact]
        public async void UpsertSettings_can_handle_complex_types()
        {
            await _svc.UpsertSettingsAsync(new MySettings { User = new User { DisplayName = "John Doe" } });
            var settings = await _svc.GetSettingsAsync<MySettings>();

            Assert.Equal("John Doe", settings.User.DisplayName);
            Assert.Equal(2, settings.Roles.Count);
        }
    }
}

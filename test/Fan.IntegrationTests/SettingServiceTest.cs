using Fan.Data;
using Fan.IntegrationTests.Base;
using Fan.Membership;
using Fan.Settings;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using Xunit;

namespace Fan.IntegrationTests
{
    /// <summary>
    /// A settings class help test <see cref="SettingService"/>.
    /// </summary>
    /// <remarks>
    /// This is a complex class with simple property and reference type and array.
    /// </remarks>
    public class MySettings : ISettings
    {
        public int Age { get; set; } = 13;
        public User User { get; set; } = new User { DisplayName = "John Smith" };
        public List<Role> Roles { get; set; } = new List<Role> {
            new Role { Name = "Admin" },
            new Role { Name = "Editor" }
        };
    }

    /// <summary>
    /// Integration tests for <see cref="SettingService"/>.
    /// </summary>
    public class SettingServiceTest : IntegrationTestBase
    {
        private ISettingService _svc;
        public SettingServiceTest()
        {
            var repo = new SqlMetaRepository(_db);
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

using Fan.Data;
using Fan.IntegrationTests.Base;
using Fan.Membership;
using Fan.Settings;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Fan.IntegrationTests.Settings
{
    /// <summary>
    /// Integration tests for <see cref="SettingService"/>.
    /// </summary>
    public class SettingServiceTest : IntegrationTestBase
    {
        private ISettingService _svc;
        public SettingServiceTest()
        {
            var repo = new SqlMetaRepository(_db);
            var logger = _loggerFactory.CreateLogger<SettingService>();
            _svc = new SettingService(repo, _cache, logger);
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

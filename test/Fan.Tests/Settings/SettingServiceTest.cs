using Fan.Settings;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Fan.Tests.Settings
{
    public class SettingServiceTest
    {
        protected Mock<ISettingRepository> _repoMock;
        protected IDistributedCache _cache;
        protected SettingService _settingSvc;
        protected ILogger<SettingService> _logger;

        public SettingServiceTest()
        {
            _repoMock = new Mock<ISettingRepository>();
            var serviceProvider = new ServiceCollection().AddMemoryCache().AddLogging().BuildServiceProvider();

            var memCacheOptions = serviceProvider.GetService<IOptions<MemoryDistributedCacheOptions>>();
            _cache = new MemoryDistributedCache(memCacheOptions);

            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            _logger = loggerFactory.CreateLogger<SettingService>();

            _settingSvc = new SettingService(_repoMock.Object, _cache, _logger);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// Have to mock and return en empty list, otherwise it get null instead of empty list.
        /// </remarks>
        [Fact]
        public async void GetAllSettings_returns_empty_list_if_no_setting_is_found()
        {
            _repoMock.Setup(repo => repo.GetAllSettingsAsync()).Returns(Task.FromResult(new List<Setting>()));
            var settings = await _settingSvc.GetAllSettingsAsync();
            Assert.NotNull(settings);
            Assert.Empty(settings);
        }

        /// <summary>
        /// Tests <see cref="SettingService.UpsertSettingsAsync{T}(T)"/> when there isn't one exists already.
        /// </summary>
        [Fact]
        public async void UpsertSettings_creates_settings_if_its_not_already_existed()
        {
            // Act
            await _settingSvc.UpsertSettingsAsync(new CoreSettings());

            // Assert:
            _repoMock.Verify(repo => repo.CreateRangeAsync(It.IsAny<List<Setting>>()), Times.Once);
        }

        /// <summary>
        /// Tests <see cref="SettingService.UpsertSettingsAsync{T}(T)"/> 
        /// </summary>
        [Fact]
        public async void UpsertSettings_updates_setting_if_setting_exists_and_a_new_value_comes_in()
        {
            // Arrange
            _repoMock.Setup(repo => repo.GetAsync("coresettings.title"))
                .Returns(Task.FromResult(new Setting() { Key = "coresettings.title", Value = "New value" }));
            _repoMock.Setup(repo => repo.GetAllSettingsAsync())
                .Returns(Task.FromResult(new List<Setting>() { new Setting() { Key = "coresettings.title", Value = "New value" } }));

            // Act
            await _settingSvc.UpsertSettingsAsync(new CoreSettings());

            // Assert: UpdateAsync() is called once
            _repoMock.Verify(repo => repo.UpdateAsync(), Times.Once);
        }

        [Fact]
        public async void UpsertSettings_does_not_update_setting_if_setting_exists_but_value_not_new()
        {
            // Arrange
            _repoMock.Setup(repo => repo.GetAsync("coresettings.title"))
                .Returns(Task.FromResult(new Setting() { Key = "coresettings.title", Value = "Fanray" }));
            _repoMock.Setup(repo => repo.GetAllSettingsAsync())
                .Returns(Task.FromResult(new List<Setting>() { new Setting() { Key = "coresettings.title", Value = "Fanray" } }));

            // Act
            await _settingSvc.UpsertSettingsAsync(new CoreSettings());

            // Assert
            _repoMock.Verify(repo => repo.UpdateAsync(), Times.Never);
        }
    }
}

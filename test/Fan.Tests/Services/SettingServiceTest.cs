using Fan.Data;
using Fan.Exceptions;
using Fan.Models;
using Fan.Services;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Xunit;

namespace Fan.Tests.Services
{
    /// <summary>
    /// Tests for <see cref="SettingService"/> class.
    /// </summary>
    public class SettingServiceTest 
    {
        protected Mock<IMetaRepository> _metaRepoMock;
        protected IDistributedCache _cache;
        protected SettingService _settingSvc;
        protected ILogger<SettingService> _loggerSettingSvc;

        public SettingServiceTest()
        {
            // repo
            _metaRepoMock = new Mock<IMetaRepository>();

            // cache
            var serviceProvider = new ServiceCollection().AddMemoryCache().AddLogging().BuildServiceProvider();
            var memCacheOptions = serviceProvider.GetService<IOptions<MemoryDistributedCacheOptions>>();
            _cache = new MemoryDistributedCache(memCacheOptions);

            // logger
            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            _loggerSettingSvc = loggerFactory.CreateLogger<SettingService>();

            _settingSvc = new SettingService(_metaRepoMock.Object, _cache, _loggerSettingSvc);
        }

        /// <summary>
        /// Tests <see cref="SettingService.CreateSettingsAsync{T}(T)"/> when there isn't one exists already.
        /// </summary>
        [Fact]
        public async void CreateSettings_Creates_Settings_If_Its_Not_Already_Existed()
        {
            // Arrange: no settings setup here

            // Act
            await _settingSvc.CreateSettingsAsync(new SiteSettings());

            // Assert
            _metaRepoMock.Verify(repo => repo.CreateAsync(It.IsAny<Meta>()), Times.Exactly(1));
        }

        /// <summary>
        /// Tests <see cref="SettingService.CreateSettingsAsync{T}(T)"/> when there isn't one exists already.
        /// </summary>
        [Fact]
        public async void CreateSettings_Throws_FanException_If_Settings_Already_Existed()
        {
            // Arrange 
            _metaRepoMock.Setup(repo => repo.GetAsync("SiteSettings"))
                .Returns(Task.FromResult(new Meta { Key = "SiteSettings", Value = JsonConvert.SerializeObject(new SiteSettings()) }));

            // Assert
            var ex = await Assert.ThrowsAsync<FanException>(() => _settingSvc.CreateSettingsAsync(new SiteSettings()));
        }

        /// <summary>
        /// Test <see cref="SettingService.GetSettingsAsync{T}(bool)"/> returns settings from cache after initial access.
        /// </summary>
        [Fact]
        public async void GetSettings_Returns_Settings_From_Cache_AfterInitialAccess()
        {
            // Arrange: existing SiteSettings
            _metaRepoMock.Setup(repo => repo.GetAsync("SiteSettings"))
                .Returns(Task.FromResult(new Meta { Key = "SiteSettings", Value = JsonConvert.SerializeObject(new SiteSettings()) }));

            // Act: when getting it for the first time, it calls ICategoryRepository and caches them
            var settings = await _settingSvc.GetSettingsAsync<SiteSettings>();

            // Assert: 
            Assert.NotNull(settings);

            // Act: gets them again, it would get them from cache and not call repo
            settings = await _settingSvc.GetSettingsAsync<SiteSettings>();

            // Assert: we called repo once
            _metaRepoMock.Verify(repo => repo.GetAsync("SiteSettings"), Times.Exactly(1));
        }

        /// <summary>
        /// Test <see cref="SettingService.UpdateSettingsAsync{T}(T)"/> throws FanException if settings not found.
        /// </summary>
        [Fact]
        public async void UpdateSettings_Throws_FanException_If_SiteSettings_Not_Found()
        {
            await Assert.ThrowsAsync<FanException>(() => _settingSvc.UpdateSettingsAsync(new SiteSettings()));
        }

        /// <summary>
        /// Test <see cref="SettingService.UpdateSettingsAsync{T}(T)"/> updates settings will call MetaRepository once.
        /// </summary>
        [Fact]
        public async void UpdateSettings_Updates_SiteSettings_Will_Call_MetaRepository()
        {
            // Arrange: existing SiteSettings
            _metaRepoMock.Setup(repo => repo.GetAsync("SiteSettings"))
                .Returns(Task.FromResult(new Meta { Key = "SiteSettings", Value = JsonConvert.SerializeObject(new SiteSettings()) }));

            // Act
            var siteSettings = await _settingSvc.GetSettingsAsync<SiteSettings>();
            siteSettings.Title = "A new site title";
            await _settingSvc.UpdateSettingsAsync(siteSettings);

            // Assert
            _metaRepoMock.Verify(repo => repo.UpdateAsync(It.IsAny<Meta>()), Times.Exactly(1));
        }
    }
}
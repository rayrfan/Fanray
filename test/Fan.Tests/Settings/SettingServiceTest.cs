using Fan.Data;
using Fan.Settings;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Fan.Tests.Settings
{
    /// <summary>
    /// Tests SettingService.
    /// </summary>
    public class SettingServiceTest
    {
        private readonly Mock<IMetaRepository> metaRepoMock;
        private readonly ISettingService settingService;

        public SettingServiceTest()
        {
            var serviceProvider = new ServiceCollection().AddMemoryCache().AddLogging().BuildServiceProvider();
            var cache = new MemoryDistributedCache(serviceProvider.GetService<IOptions<MemoryDistributedCacheOptions>>());
            var logger = serviceProvider.GetService<ILoggerFactory>().CreateLogger<SettingService>();

            metaRepoMock = new Mock<IMetaRepository>();
            settingService = new SettingService(metaRepoMock.Object, cache, logger);
        }

        /// <summary>
        /// GetSettings is able to get setting records from db and serializes them to an ISettings object.
        /// </summary>
        [Fact]
        public async void GetSettingsAsync_retrieves_settings_from_db_and_serializes_to_object()
        {
            // Given an existing setting title
            // Note GetSettings only calls repo.FindAsync
            var title = "Old value";
            metaRepoMock.Setup(repo => repo.FindAsync(m => m.Type == EMetaType.Setting))
                .Returns(Task.FromResult((new List<Meta>() { new Meta() { Key = "coresettings.title", Value = title } }).AsEnumerable()));

            // When retrieving CoreSettings
            var settings = await settingService.GetSettingsAsync<CoreSettings>();

            // Then the title property is the value from db
            Assert.Equal(title, settings.Title);
        }

        /// <summary>
        /// When UpsertSettings with no existing settings, CreateRange is called once but Update 
        /// is never called.
        /// </summary>
        [Fact]
        public async void UpsertSettings_creates_settings_if_its_not_already_existed()
        {
            // When UpsertSettings with no existing settings
            await settingService.UpsertSettingsAsync(new CoreSettings());

            // Then CreateRange is called once but Update is never called
            metaRepoMock.Verify(repo => repo.CreateRangeAsync(It.IsAny<IEnumerable<Meta>>()), Times.Once);
            metaRepoMock.Verify(repo => repo.UpdateAsync(It.IsAny<IEnumerable<Meta>>()), Times.Never);
        }

        /// <summary>
        /// UpsertSettingsAsync will update a meta record if setting exists, it will inserts a meta
        /// record is setting is new.
        /// </summary>
        /// <remarks>
        /// Settings are inserted and updated in one shot.
        /// </remarks>
        [Fact]
        public async void UpsertSettings_updates_meta_if_exists_inserts_meta_if_new()
        {
            // Given an existing setting title in db
            metaRepoMock.Setup(repo => repo.GetAsync("coresettings.title", EMetaType.Setting))
                .Returns(Task.FromResult(new Meta() { Key = "coresettings.title", Value = "Old value" }));
            metaRepoMock.Setup(repo => repo.FindAsync(m => m.Type == EMetaType.Setting))
                .Returns(Task.FromResult((new List<Meta>() { new Meta() { Key = "coresettings.title", Value = "Old value" } }).AsEnumerable()));

            // When Upsert CoreSettings
            await settingService.UpsertSettingsAsync(new CoreSettings());

            // Then because there are both new and existing settings
            // CreateRange and Update each is called once
            metaRepoMock.Verify(repo => repo.CreateRangeAsync(It.IsAny<IEnumerable<Meta>>()), Times.Once);
            metaRepoMock.Verify(repo => repo.UpdateAsync(It.IsAny<IEnumerable<Meta>>()), Times.Once);
        }

        /// <summary>
        /// Given a setting "title" with value "Fanray" in db, when we UpsertSetting with "title"
        /// with same value "Fanray", UpsertSettings will not do update.
        /// </summary>
        [Fact]
        public async void UpsertSettings_does_not_update_meta_if_exists_setting_value_is_same()
        {
            // Given an existing setting title with value "Fanray" that is the same as 
            // what we are about to give to UpsertSettings
            metaRepoMock.Setup(repo => repo.GetAsync("coresettings.title", EMetaType.Setting))
                .Returns(Task.FromResult(new Meta() { Key = "coresettings.title", Value = "Fanray" }));
            metaRepoMock.Setup(repo => repo.FindAsync(m => m.Type == EMetaType.Setting))
                .Returns(Task.FromResult((new List<Meta>() { new Meta() { Key = "coresettings.title", Value = "Fanray" } }).AsEnumerable()));

            // When we UpsertSettings of CoreSettings with title of the same value "Fanray"
            await settingService.UpsertSettingsAsync(new CoreSettings());

            // Then Update is never called, CreateRange is called once
            metaRepoMock.Verify(repo => repo.CreateRangeAsync(It.IsAny<IEnumerable<Meta>>()), Times.Once);
            metaRepoMock.Verify(repo => repo.UpdateAsync(It.IsAny<Meta>()), Times.Never);
        }

        /// <summary>
        /// UpsertSettings can take in settings object where its property can be a complex type.
        /// </summary>
        /// <remarks>
        /// For example CoreSettings has "home" which is a Nav type.
        /// </remarks>
        [Fact]
        public async void UpsertSettings_can_handle_complex_types()
        {
            // Given home setting which is a Nav type
            metaRepoMock.Setup(repo => repo.GetAsync("coresettings.home", EMetaType.Setting))
                .Returns(Task.FromResult(new Meta() { Key = "coresettings.home", Value = "{\"id\":1,\"type\":0}" }));

            // When retriving settings
            var settings = await settingService.GetSettingsAsync<CoreSettings>();

            // Then Home property is restored to the Nav type
            Assert.Equal(1, settings.Home.Id);
            Assert.Equal(Navigation.ENavType.App, settings.Home.Type);
        }
    }
}

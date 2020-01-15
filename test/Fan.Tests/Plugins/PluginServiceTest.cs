using Fan.Data;
using Fan.Plugins;
using Fan.Tests.SysPlugins;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Fan.Tests.Plugins
{
    public class PluginServiceTest
    {
        private readonly IPluginService pluginService;
        private readonly Mock<IMetaRepository> metaRepoMock = new Mock<IMetaRepository>();

        public PluginServiceTest()
        {
            // cache and logger
            var serviceProvider = new ServiceCollection().AddMemoryCache().AddLogging().BuildServiceProvider();
            var cache = new MemoryDistributedCache(serviceProvider.GetService<IOptions<MemoryDistributedCacheOptions>>());
            var logger = serviceProvider.GetService<ILoggerFactory>().CreateLogger<PluginService>();

            // mock ContentRootPath to return current dir
            var hostingEnvMock = new Mock<IWebHostEnvironment>();
            hostingEnvMock.Setup(env => env.ContentRootPath).Returns(Directory.GetCurrentDirectory());

            // service
            pluginService = new PluginService(hostingEnvMock.Object, cache, metaRepoMock.Object, logger);
        }

        /// <summary>
        /// When an in-active plugin is activated, repo updates it.
        /// </summary>
        [Fact]
        public async void ActivatePluginAsync_on_plugin_that_is_not_active()
        {
            // Arrange a plugin meta
            var key = "myplugin";
            var myPlugin = new MyPlugin();
            var meta = new Meta { Id = 1, Key = key, Value = JsonConvert.SerializeObject(myPlugin), Type = EMetaType.Plugin };
            metaRepoMock.Setup(repo => repo.GetAsync(key, EMetaType.Plugin)).Returns(Task.FromResult(meta));
            metaRepoMock.Setup(repo => repo.GetAsync(1)).Returns(Task.FromResult(meta));

            // Act when user activates it
            var plugin = await pluginService.ActivatePluginAsync(myPlugin.Folder);

            // Assert the plugin is updated at datasource
            metaRepoMock.Verify(repo => repo.UpdateAsync(It.IsAny<Meta>()), Times.Once);
        }

        /// <summary>
        /// When a plugin is activated for the first time, a meta recorded is created.
        /// </summary>
        [Fact]
        public async void ActivatePluginAsync_on_plugin_never_activated()
        {
            // Given no meta record exist for MyPlugin
            var key = "myplugin";
            metaRepoMock.Setup(repo => repo.GetAsync(key, EMetaType.Plugin)).Returns(Task.FromResult((Meta)null));
            // suppose user activates MyPlugin, mock an active plugin to be returned by repo create
            var myPlugin = new MyPlugin
            {
                Active = true
            };
            var meta = new Meta { Id = 1, Key = key, Value = JsonConvert.SerializeObject(myPlugin), Type = EMetaType.Plugin };
            metaRepoMock.Setup(repo => repo.CreateAsync(It.Is<Meta>(m => m.Key == key))).Returns(Task.FromResult(meta));
            metaRepoMock.Setup(repo => repo.GetAsync(1)).Returns(Task.FromResult(meta));

            // When user activates it
            var plugin = await pluginService.ActivatePluginAsync(myPlugin.Folder);

            // Then 
            // repo create is called once
            metaRepoMock.Verify(repo => repo.CreateAsync(It.IsAny<Meta>()), Times.Once);
            // the returned plugin is active
            Assert.Equal(1, plugin.Id);
            Assert.True(plugin.Active);
        }

        /// <summary>
        /// User can de-activates an active plugin.
        /// </summary>
        [Fact]
        public async void DeactivatePluginAsync_turns_plugin_active_to_false()
        {
            // Arrange a plugin meta
            var key = "myplugin";
            var myPlugin = new MyPlugin { Active = true };
            var meta = new Meta { Id = 1, Key = key, Value = JsonConvert.SerializeObject(myPlugin), Type = EMetaType.Plugin };
            metaRepoMock.Setup(repo => repo.GetAsync(key, EMetaType.Plugin)).Returns(Task.FromResult(meta));
            metaRepoMock.Setup(repo => repo.GetAsync(1)).Returns(Task.FromResult(meta));

            // Act when user de-activates it
            await pluginService.DeactivatePluginAsync(1);

            // Assert the plugin is updated at datasource
            metaRepoMock.Verify(repo => repo.UpdateAsync(It.IsAny<Meta>()), Times.Once);
        }

        /// <summary>
        /// The theme layout uses calls this method through the use of taghelper. If a user copies 
        /// over a new plugin, this method will activates it and returns all active plugins.
        /// </summary>
        [Fact]
        public async void GetActivePluginsAsync_returns_all_active_plugins_and_activates_any_new_plugins()
        {
            // Arrange             
            // an active plugin in db
            var key = "myplugin";
            var myPlugin = new MyPlugin { Active = true };
            var meta = new Meta { Id = 1, Key = key, Value = JsonConvert.SerializeObject(myPlugin), Type = EMetaType.Plugin };
            IEnumerable<Meta> activePluginMetas = new List<Meta> { meta };
            metaRepoMock.Setup(repo => repo.FindAsync(m => m.Type == EMetaType.Plugin)).Returns(Task.FromResult(activePluginMetas));

            myPlugin.Active = true;
            var metaActive = new Meta { Id = 1, Key = key, Value = JsonConvert.SerializeObject(myPlugin), Type = EMetaType.Plugin };
            metaRepoMock.Setup(repo => repo.GetAsync(1)).Returns(Task.FromResult(metaActive));

            // suppose a sys plugin is copied over then mock its meta to be returned
            var key2 = "mysysplugin";
            var mySysPlugin = new MySysPlugin();
            var meta2 = new Meta { Id = 2, Key = key2, Value = JsonConvert.SerializeObject(mySysPlugin), Type = EMetaType.Plugin };
            metaRepoMock.Setup(repo => repo.CreateAsync(It.Is<Meta>(m => m.Key == key2))).Returns(Task.FromResult(meta2));

            mySysPlugin.Active = true;
            var meta2Active = new Meta { Id = 2, Key = key2, Value = JsonConvert.SerializeObject(mySysPlugin), Type = EMetaType.Plugin };
            metaRepoMock.Setup(repo => repo.GetAsync(2)).Returns(Task.FromResult(meta2Active));

            // Act
            var activePlugins = await pluginService.GetActivePluginsAsync();

            // Assert
            Assert.Equal(2, activePlugins.Count());

            var mySysPluginAgain = activePlugins.ToList()[0];
            var myPluginAgain = activePlugins.ToList()[1];

            // plugins are of their specific types
            Assert.True(mySysPluginAgain.GetType() == typeof(MySysPlugin));
            Assert.True(myPluginAgain.GetType() == typeof(MyPlugin));

            // plugins are active
            Assert.True(mySysPluginAgain.Active); 
            Assert.True(myPluginAgain.Active); 

            Assert.Equal(mySysPlugin.Folder, mySysPluginAgain.Folder);
            Assert.Equal(myPlugin.Folder, myPluginAgain.Folder);
        }

        /// <summary>
        /// The admin plugins page displays all installed plugins and system plugins.
        /// </summary>
        [Fact]
        public async void GetManifestsAsync_displays_all_plugins_in_admin()
        {
            // Given two plugin meta records
            var key1 = "myplugin";
            var meta1 = new Meta { Id = 1, Key = key1, Value = JsonConvert.SerializeObject(new MyPlugin()), Type = EMetaType.Plugin };
            var key2 = "mysysplugin";
            var meta2 = new Meta { Id = 2, Key = key2, Value = JsonConvert.SerializeObject(new MySysPlugin()), Type = EMetaType.Plugin };

            metaRepoMock.Setup(repo => repo.GetAsync(key1, EMetaType.Plugin)).Returns(Task.FromResult(meta1));
            metaRepoMock.Setup(repo => repo.GetAsync(key2, EMetaType.Plugin)).Returns(Task.FromResult(meta2));
            metaRepoMock.Setup(repo => repo.GetAsync(1)).Returns(Task.FromResult(meta1));
            metaRepoMock.Setup(repo => repo.GetAsync(2)).Returns(Task.FromResult(meta2));

            // When admin plugins page loads manifests
            var plugins = await pluginService.GetManifestsAsync();

            // Then 
            // all manifest files are returned
            Assert.Equal(2, plugins.Count());
            // their names match what's in the plugin.json from current dir
            Assert.Equal("My Plugin", plugins.ToList()[0].Name);
            Assert.Equal("My System Plugin", plugins.ToList()[1].Name);
        }

        /// <summary>
        /// When user saves a plugin's settings that is never activated, a meta record is created.
        /// </summary>
        [Fact]
        public async void UpsertPluginAsync_creates_meta_on_plugin_never_activated()
        {
            // Given a plugin that is never activated
            var key = "myplugin";
            var value = JsonConvert.SerializeObject(new MyPlugin());
            var metaCreating = new Meta { Key = key, Value = value, Type = EMetaType.Plugin };
            var metaCreated = new Meta { Id = 1, Key = key, Value = value, Type = EMetaType.Plugin };

            metaRepoMock.Setup(repo => repo.GetAsync(key, EMetaType.Plugin)).Returns(Task.FromResult((Meta)null));
            metaRepoMock.Setup(repo => repo.CreateAsync(It.Is<Meta>(m => m.Value == value))).Returns(Task.FromResult(metaCreated));

            // When user saves it
            var id = await pluginService.UpsertPluginAsync(new MyPlugin());

            // Then a meta record is created
            Assert.Equal(1, id);
            metaRepoMock.Verify(repo => repo.CreateAsync(It.IsAny<Meta>()), Times.Once);
            metaRepoMock.Verify(repo => repo.UpdateAsync(It.IsAny<Meta>()), Times.Never);
        }

        /// <summary>
        /// User can update a plugin's settings without it being active.
        /// </summary>
        [Fact]
        public async void UpsertPluginAsync_updates_inactive_plugin_settings()
        {
            // Given an in-active plugin on the Plugins page
            var key = "myplugin";
            var value = @"{""age"":15,""name"":""John"",""folder"":""MyPlugin""}";
            var metaCreating = new Meta { Key = key, Value = value, Type = EMetaType.Plugin };
            var metaCreated = new Meta { Id = 1, Key = key, Value = value, Type = EMetaType.Plugin };
            metaRepoMock.Setup(repo => repo.GetAsync(key, EMetaType.Plugin)).Returns(Task.FromResult(metaCreated));

            // When user updates my plugin
            var myPlugin = new MyPlugin { Name = "John", Folder = "MyPlugin" };
            var id = await pluginService.UpsertPluginAsync(myPlugin);

            // Then plugin is not active but the settings have been updated
            Assert.Equal(1, id);
            metaRepoMock.Verify(repo => repo.CreateAsync(It.IsAny<Meta>()), Times.Never);
            metaRepoMock.Verify(repo => repo.UpdateAsync(It.IsAny<Meta>()), Times.Once);
        }
    }
}

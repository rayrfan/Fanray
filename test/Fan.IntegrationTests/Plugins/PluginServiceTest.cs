using Fan.Data;
using Fan.Exceptions;
using Fan.IntegrationTests.Base;
using Fan.Plugins;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace Fan.IntegrationTests.Plugins
{
    public class PluginServiceTest : IntegrationTestBase
    {
        private const string MY_PLUGIN = "MyPlugin";
        private const string MY_SYSPLUGIN = "MySysPlugin";

        private readonly PluginService pluginService;
        private readonly SqlMetaRepository metaRepository;

        public PluginServiceTest()
        {
            // repo
            metaRepository = new SqlMetaRepository(_db);

            // IHostingEnvironment
            var workingDirectory = Environment.CurrentDirectory;
            var projectDirectory = Directory.GetParent(workingDirectory).Parent.Parent.FullName;
            var env = new Mock<IHostingEnvironment>();
            env.Setup(m => m.ContentRootPath).Returns(projectDirectory);

            // logger
            var loggerPluginSvc = _loggerFactory.CreateLogger<PluginService>();

            // svc
            pluginService = new PluginService(env.Object, _cache, metaRepository, loggerPluginSvc);
        }

        /// <summary>
        /// The admin plugins page display all installed plugins and system plugins.
        /// </summary>
        [Fact]
        public async void Admin_plugins_page_displays_all_installed_plugins_and_system_plugins()
        {
            var plugins = await pluginService.GetManifestsAsync();

            Assert.Equal(2, plugins.Count());
            Assert.Equal("My Plugin", plugins.ToList()[0].Name);
            Assert.Equal("My System Plugin", plugins.ToList()[1].Name);
        }

        /// <summary>
        /// On the Plugins page user can update a plugin's settings without activate it.
        /// </summary>
        [Fact]
        public async void User_can_modify_plugin_settings_without_activating_the_plugin()
        {
            // Given a plugin on the Plugins page
            var plugin = new MyPlugin { Name = "Ray", Folder = MY_PLUGIN };

            // When user updates it
            var id = await pluginService.UpsertPluginAsync(plugin);

            // Then plugin is not active but the settings have been updated
            var pluginAgain = (MyPlugin) await pluginService.GetExtensionAsync(id);
            Assert.False(pluginAgain.Active);
            Assert.Equal("Ray", pluginAgain.Name);
        }

        /// <summary>
        /// When user saves a plugin's settings that is never activated, a meta record is created.
        /// </summary>
        [Fact]
        public async void Saving_plugin_that_is_never_activated_creates_a_meta_record()
        {
            // Given a plugin that has never been activated
            var plugin = new MyPlugin { Name = "Ray", Folder = MY_PLUGIN };

            // When user updates it
            var id = await pluginService.UpsertPluginAsync(plugin);

            // Then there will be a plugin meta
            var pluginMeta = await metaRepository.GetAsync(id);
            Assert.NotNull(pluginMeta);
        }

        /// <summary>
        /// When a user activates a plugin, there will be a plugin meta and its Active prop is true.
        /// 
        /// </summary>
        [Fact]
        public async void When_user_activates_plugin_a_meta_record_is_created_and_plugin_active_is_true()
        {
            // Given a plugin and when user activates it
            var id = await pluginService.ActivatePluginAsync(MY_PLUGIN);

            // Then there will be a plugin meta
            var pluginMeta = await metaRepository.GetAsync(id);
            Assert.NotNull(pluginMeta);

            // And the plugin's Active property is true
            var plugin = await pluginService.GetExtensionAsync(id);
            Assert.True(plugin.Active);
        }

        /// <summary>
        /// SysPlugins also require activation and are activated at install time.
        /// </summary>
        [Fact]
        public async void System_plugin_also_need_to_be_activated()
        {
            // Given a plugin and when the system activates it
            var id = await pluginService.ActivatePluginAsync(MY_SYSPLUGIN);

            // Then there will be a plugin meta
            var pluginMeta = await metaRepository.GetAsync(id);
            Assert.NotNull(pluginMeta);

            // And the plugin's Active property is true
            var plugin = await pluginService.GetExtensionAsync(id);
            Assert.True(plugin.Active);
        }
    }
}

//using Fan.Data;
//using Fan.IntegrationTests.Base;
//using Fan.Plugins;
//using Microsoft.AspNetCore.Hosting;
//using Microsoft.Extensions.Logging;
//using Moq;
//using Newtonsoft.Json;
//using Serilog;
//using System;
//using System.IO;
//using System.Linq;
//using Xunit;
//using Xunit.Abstractions;

//namespace Fan.IntegrationTests.Plugins
//{
//    public class PluginServiceTest : IntegrationTestBase
//    {
//        private const string MY_PLUGIN_FOLDER = "MyPlugin";

//        private readonly PluginService pluginService;
//        private readonly SqlMetaRepository metaRepository;
//        private readonly Serilog.ILogger _output;

//        public PluginServiceTest(ITestOutputHelper output)
//        {
//            // repo
//            metaRepository = new SqlMetaRepository(_db);

//            // IHostingEnvironment
//            var workingDirectory = Environment.CurrentDirectory;
//            var projectDirectory = Directory.GetParent(workingDirectory).Parent.Parent.FullName;
//            var env = new Mock<IHostingEnvironment>();
//            env.Setup(m => m.ContentRootPath).Returns(projectDirectory);

//            // logger
//            var loggerPluginSvc = _loggerFactory.CreateLogger<PluginService>();

//            _output = new LoggerConfiguration()
//                .MinimumLevel.Verbose()
//                .WriteTo.TestOutput(output, Serilog.Events.LogEventLevel.Verbose)
//                .CreateLogger()
//                .ForContext<PluginServiceTest>();

//            // svc
//            pluginService = new PluginService(env.Object, _cache, metaRepository, loggerPluginSvc);
//        }

//        /// <summary>
//        /// The admin plugins page display all installed plugins.
//        /// </summary>
//        [Fact]
//        public async void Admin_panel_plugins_page_displays_all_installed_plugins_information()
//        {
//            _output.Information("The admin plugins page display all installed plugins.");

//            var plugins = await pluginService.GetManifestsAsync();

//            Assert.Single(plugins);
//            Assert.Equal("My Plugin", plugins.ToList()[0].Name);
//        }

//        /// <summary>
//        /// When a user activates a plugin, there will be a plugin meta and its Active prop is true.
//        /// </summary>
//        [Fact]
//        public async void When_user_activates_a_plugin_two_meta_records_could_be_created()
//        {
//            _output.Information("When a user activates a plugin, there will be a plugin meta and its Active prop is true.");

//            // Given a plugin and when user activates it
//            var id = await pluginService.ActivatePluginAsync(MY_PLUGIN_FOLDER);

//            // Then there will be a plugin meta
//            var pluginMeta = await metaRepository.GetAsync(id);
//            Assert.NotNull(pluginMeta);

//            // And the plugin's Active property is true
//            var plugin = await pluginService.GetExtensionAsync(id);
//            Assert.True(plugin.Active);
//        }

//        /// <summary>
//        /// Plugin settings link is only shown on active plugins.
//        /// </summary>
//        [Fact]
//        public async void Plugin_settings_link_is_only_shown_on_active_plugins()
//        {
//            _output.Information("Plugin settings link is only shown on active plugins.");

//            // Given a plugin and when user activates it 
//            await pluginService.ActivatePluginAsync(MY_PLUGIN_FOLDER);

//            // Then the manifest info show it as active
//            var list = await pluginService.GetManifestsAsync();
//            Assert.True(list.ToList()[0].Active);
//        }

//        [Fact]
//        public async void User_can_update_plugin_settings()
//        {
//            _output.Information("User can update plugin settings.");

//            // Given an active plugin
//            var plugin = new MyPlugin
//            {
//                Folder = MY_PLUGIN_FOLDER
//            };
//            var id = await pluginService.ActivatePluginAsync(plugin.Folder);

//            // When user updates it
//            plugin.Name = "Ray";
//            await pluginService.UpdatePluginAsync(plugin);

//            // Then 
//            var meta = await metaRepository.GetAsync(id);
//            var pluginAgain = JsonConvert.DeserializeObject<MyPlugin>(meta.Value);

//            Assert.Equal("Ray", pluginAgain.Name);
//        }
//    }
//}

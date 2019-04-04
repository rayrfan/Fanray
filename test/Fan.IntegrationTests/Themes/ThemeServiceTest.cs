using Fan.Data;
using Fan.Exceptions;
using Fan.IntegrationTests.Base;
using Fan.Settings;
using Fan.Themes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Fan.IntegrationTests.Themes
{
    public class ThemeServiceTest : IntegrationTestBase
    {
        private readonly IThemeService _svc;
        private IMetaRepository _metaRepo;

        public ThemeServiceTest()
        {
            // meta repo
            _metaRepo = new SqlMetaRepository(_db);

            // default CoreSettings
            var settingSvcMock = new Mock<ISettingService>();
            settingSvcMock.Setup(svc => svc.GetSettingsAsync<CoreSettings>()).Returns(Task.FromResult(new CoreSettings()));

            // set ContentRootPath to "Fan.IntegrationTests"
            var workingDirectory = Environment.CurrentDirectory;
            var projectDirectory = Directory.GetParent(workingDirectory).Parent.Parent.FullName;
            var env = new Mock<IHostingEnvironment>();
            env.Setup(m => m.ContentRootPath).Returns(projectDirectory);

            // logger
            var logger = _loggerFactory.CreateLogger<ThemeService>();

            _svc = new ThemeService(settingSvcMock.Object, env.Object, _cache, _metaRepo, logger);
        }

        /// <summary>
        /// Activating a theme will register the theme and the theme defined widget areas.
        /// </summary>
        [Fact]
        public async void When_user_activates_a_theme_the_theme_and_its_defined_widget_areas_will_be_registered()
        {
            // When user activates a theme
            await _svc.ActivateThemeAsync("Clarity");

            // Then the theme is registered
            var metaTheme = await _metaRepo.GetAsync("clarity", EMetaType.Theme);
            var metaArea = await _metaRepo.GetAsync("clarity-my-area", EMetaType.WidgetAreaByTheme);

            Assert.Equal("clarity", metaTheme.Key);
            Assert.Equal("clarity-my-area", metaArea.Key);
        }

        /// <summary>
        /// A theme's folder name can only be alphanumeric, dash, underscore; it cannot contain
        /// characters like '/', ' ', '.' etc.
        /// </summary>
        /// <param name="folder"></param>
        [Theory]
        [InlineData("test.")]
        [InlineData("test one")]
        [InlineData("/test")]
        public async void Invalid_theme_folder_name_cannot_be_activated(string folder)
        {
            await Assert.ThrowsAsync<FanException>(() => _svc.ActivateThemeAsync(folder));
        }

        /// <summary>
        /// On the admin panel themes page, all installed themes will display there.
        /// </summary>
        [Fact]
        public async void Admin_panel_themes_page_displays_all_installed_themes()
        {
            // When I have only 1 installed theme
            var themes = await _svc.GetInstalledThemesInfoAsync();

            // Then 1 theme will be available
            Assert.Single(themes);
        }

        [Fact]
        public async void A_default_theme_named_clarity_is_always_available()
        {
            // When system retrieves installed themes
            var themes = await _svc.GetInstalledThemesInfoAsync();

            // Then the default Clarity theme should be available
            Assert.Contains(themes, t => t.Name == "Clarity");
            Assert.Contains(themes, t => t.Folder.Equals("clarity", StringComparison.CurrentCultureIgnoreCase));
        }

        /// <summary>
        /// A "theme.json" file at the root of each theme's folder will provide
        /// widget areas the theme uses.
        /// </summary>
        [Fact]
        public async void Themes_will_provide_widget_areas_information_they_use()
        {
            // Given a "Themes/Clarity" directory that contains a "theme.json" file
            // When Admin Panel Themes page retrieves themes info
            var themes = await _svc.GetInstalledThemesInfoAsync();

            // Then the theme contains 3 areas 
            var areas = themes[0].WidgetAreas;
            Assert.Equal(3, areas.Length);
            Assert.True(areas[0].Id == "blog-sidebar1");
            Assert.True(areas[1].Id == "blog-sidebar2");
        }
    }
}

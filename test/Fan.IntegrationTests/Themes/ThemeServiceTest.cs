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

        public ThemeServiceTest()
        {
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

            _svc = new ThemeService(settingSvcMock.Object, env.Object, _cache, logger);
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

            // Then the theme contains 2 areas 
            var areas = themes[0].WidgetAreas;
            Assert.Equal(2, areas.Length);
            Assert.True(areas[0] == "blog-sidebar1");
            Assert.True(areas[1] == "blog-sidebar2");
        }
    }
}

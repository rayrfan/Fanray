using Fan.Data;
using Fan.Exceptions;
using Fan.Themes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Fan.Tests.Themes
{
    public class ThemeServiceTest
    {
        private readonly IThemeService themeService;
        private readonly Mock<IMetaRepository> metaRepoMock = new Mock<IMetaRepository>();

        public ThemeServiceTest()
        {
            // cache and logger
            var serviceProvider = new ServiceCollection().AddMemoryCache().AddLogging().BuildServiceProvider();
            var cache = new MemoryDistributedCache(serviceProvider.GetService<IOptions<MemoryDistributedCacheOptions>>());
            var logger = serviceProvider.GetService<ILoggerFactory>().CreateLogger<ThemeService>();

            // mock ContentRootPath to return current dir
            var hostingEnvMock = new Mock<IWebHostEnvironment>();
            hostingEnvMock.Setup(env => env.ContentRootPath).Returns(Directory.GetCurrentDirectory());

            // theme service
            themeService = new ThemeService(hostingEnvMock.Object, cache, metaRepoMock.Object, logger);
        }

        /// <summary>
        /// When activating a theme if its meta record not exist yet, it will create it.
        /// </summary>
        [Fact]
        public async void ActivateThemeAsync_registers_theme_if_not_already_exist()
        {
            // Given
            // "mytheme" meta record not exist
            var myTheme = new MyTheme();
            metaRepoMock.Setup(repo => repo.GetAsync("mytheme", EMetaType.Theme)).Returns(Task.FromResult((Meta) null));

            // When 
            await themeService.ActivateThemeAsync(myTheme.Folder);

            // Then
            // mytheme meta record is create
            var metaTheme = new Meta
            {
                Key = "mytheme",
                Value = "",
                Type = EMetaType.Theme
            };
            metaRepoMock.Verify(repo =>
               repo.CreateAsync(It.Is<Meta>(m =>
                       m.Key == metaTheme.Key &&
                       m.Value == metaTheme.Value &&
                       m.Type == metaTheme.Type)),
               Times.Once);
        }

        /// <summary>
        /// When activating a theme if any theme-defined widget areas not exist, it'll create them.
        /// </summary>
        [Fact]
        public async void ActivateThemeAsync_registers_any_theme_defined_widget_areas()
        {
            // Arrange
            // mytheme meta reocrd
            var key = "mytheme";
            var myTheme = new MyTheme();
            var meta = new Meta { Id = 1, Key = key, Value = JsonConvert.SerializeObject(myTheme), Type = EMetaType.Plugin };
            metaRepoMock.Setup(repo => repo.GetAsync(key, EMetaType.Theme)).Returns(Task.FromResult(meta));
            // theme defined widget areas not exist
            metaRepoMock.Setup(repo => repo.GetAsync(It.IsAny<string>(), EMetaType.WidgetAreaByTheme))
                .Returns(Task.FromResult((Meta)null));

            // Act activate a theme
            await themeService.ActivateThemeAsync(myTheme.Folder);

            // Assert a theme defined widget area is inserted
            var metaWidgetArea = new Meta
            {
                Key = "mytheme-my-area",
                Value = "{\"Id\":\"my-area\",\"WidgetIds\":[]}",
                Type = EMetaType.WidgetAreaByTheme
            };
            metaRepoMock.Verify(repo => 
                repo.CreateAsync(It.Is<Meta>(m => 
                        m.Key == metaWidgetArea.Key && 
                        m.Value == metaWidgetArea.Value && 
                        m.Type == metaWidgetArea.Type)), 
                Times.Once);
        }

        /// <summary>
        /// An extension's folder allows "a-zA-Z", "_", "-", "." and numbers.
        /// </summary>
        /// <param name="folder"></param>
        [Theory]
        [InlineData("test one")]
        [InlineData("/test")]
        public async void ActivateThemeAsync_throws_FanException_with_invalid_theme_folder_name(string folder)
        {
            await Assert.ThrowsAsync<FanException>(() => themeService.ActivateThemeAsync(folder));
        }

        /// <summary>
        /// On the admin panel themes page, all installed themes will display there.
        /// </summary>
        [Fact]
        public async void GetManifestsAsync_returns_installed_themes_with_widget_areas_info()
        {
            // Given 1 installed MyTheme
            // When admin panel themes page displays themes, this will return all
            var themes = await themeService.GetManifestsAsync();

            // Then 1 theme is available
            Assert.Single(themes);
            Assert.Equal("MyTheme", themes.ToList()[0].Folder);
            Assert.Equal("My Theme", themes.ToList()[0].Name);

            // and the theme contains 3 areas 
            var areas = themes.ToList()[0].WidgetAreas;
            Assert.Equal(3, areas.Length);
            Assert.True(areas[0].Id == "blog-sidebar1");
            Assert.True(areas[1].Id == "blog-sidebar2");
        }
    }
}

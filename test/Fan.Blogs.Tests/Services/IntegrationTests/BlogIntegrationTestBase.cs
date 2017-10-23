using Fan.Blogs.Data;
using Fan.Blogs.Helpers;
using Fan.Blogs.Models;
using Fan.Blogs.Services;
using Fan.Blogs.Tests.Data;
using Fan.Models;
using Fan.Services;
using Fan.Shortcodes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Threading.Tasks;

namespace Fan.Blogs.Tests.Services.IntegrationTests
{
    /// <summary>
    /// Blog integration test base.
    /// </summary>
    public class BlogIntegrationTestBase : BlogDataTestBase
    {
        protected BlogService _blogSvc;
        protected Mock<ISettingService> _settingSvcMock;
        protected Mock<IHostingEnvironment> _envMock;
        protected ILoggerFactory _loggerFactory;

        public BlogIntegrationTestBase()
        {
            // repos
            var catRepo = new SqlCategoryRepository(_db);
            var tagRepo = new SqlTagRepository(_db);
            var postRepo = new SqlPostRepository(_db);
            var mediaRepo = new SqlMediaRepository(_db);

            // Env
            _envMock = new Mock<IHostingEnvironment>();

            // SettingService mock
            _settingSvcMock = new Mock<ISettingService>();
            _settingSvcMock.Setup(svc => svc.GetSettingsAsync<SiteSettings>(false)).Returns(Task.FromResult(new SiteSettings()));
            _settingSvcMock.Setup(svc => svc.GetSettingsAsync<BlogSettings>(false)).Returns(Task.FromResult(new BlogSettings()));

            // Cache
            var serviceProvider = new ServiceCollection().AddMemoryCache().AddLogging().BuildServiceProvider();
            var memCacheOptions = serviceProvider.GetService<IOptions<MemoryDistributedCacheOptions>>();
            var cache = new MemoryDistributedCache(memCacheOptions);

            // LoggerFactory
            _loggerFactory = serviceProvider.GetService<ILoggerFactory>();

            // Mapper
            var mapper = BlogUtil.Mapper;

            // Shortcode
            var shortcodeSvc = new Mock<IShortcodeService>();

            var loggerBlogSvc = _loggerFactory.CreateLogger<BlogService>();
            _blogSvc = new BlogService(_settingSvcMock.Object, catRepo, postRepo, tagRepo, mediaRepo, _envMock.Object, cache, loggerBlogSvc, mapper, shortcodeSvc.Object);
        }
    }
}

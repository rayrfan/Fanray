using AutoMapper;
using Fan.Blogs.Data;
using Fan.Blogs.Helpers;
using Fan.Blogs.Services;
using Fan.Settings;
using Fan.Shortcodes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Fan.Blogs.Tests.Services
{
    /// <summary>
    /// Base class for <see cref="BlogService"/> unit tests.
    /// </summary>
    public class BlogUnitTestBase
    {
        protected Mock<IPostRepository> _postRepoMock;
        protected Mock<ISettingRepository> _settingRepoMock;
        protected Mock<ICategoryRepository> _catRepoMock;
        protected Mock<ITagRepository> _tagRepoMock;
        protected Mock<IMediaRepository> _mediaRepoMock;
        protected BlogService _blogSvc;
        protected SettingService _settingSvc;
        protected IMapper _mapper;
        protected IDistributedCache _cache;
        protected ILogger<BlogService> _loggerBlogSvc;
        protected ILogger<SettingService> _loggerSettingSvc;

        /// <summary>
        /// Base constructor which will be called first for each test in derived test classes, thus
        /// setting up mocked repos and components here.
        /// </summary>
        public BlogUnitTestBase()
        {
            // repos
            _postRepoMock = new Mock<IPostRepository>();
            _settingRepoMock = new Mock<ISettingRepository>();
            _catRepoMock = new Mock<ICategoryRepository>();
            _tagRepoMock = new Mock<ITagRepository>();
            _mediaRepoMock = new Mock<IMediaRepository>();

            // env 
            var envMock = new Mock<IHostingEnvironment>();

            // cache
            var serviceProvider = new ServiceCollection().AddMemoryCache().AddLogging().BuildServiceProvider();
            var memCacheOptions = serviceProvider.GetService<IOptions<MemoryDistributedCacheOptions>>();
            _cache = new MemoryDistributedCache(memCacheOptions);

            // logger
            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            _loggerBlogSvc = loggerFactory.CreateLogger<BlogService>();
            _loggerSettingSvc = loggerFactory.CreateLogger<SettingService>();

            // mapper
            _mapper = BlogUtil.Mapper;

            // shortcode
            var shortcodeSvc = new Mock<IShortcodeService>();

            // svc
            _settingSvc = new SettingService(_settingRepoMock.Object, _cache, _loggerSettingSvc);
            _blogSvc = new BlogService(_settingSvc, 
                _catRepoMock.Object, 
                _postRepoMock.Object, 
                _tagRepoMock.Object,
                _mediaRepoMock.Object,
                envMock.Object,
                _cache, 
                _loggerBlogSvc, 
                _mapper,
                shortcodeSvc.Object);
        }
    }
}

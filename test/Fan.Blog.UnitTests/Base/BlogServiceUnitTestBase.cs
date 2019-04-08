using Fan.Blog.Data;
using Fan.Blog.Helpers;
using Fan.Blog.Models;
using Fan.Blog.Services;
using Fan.Blog.Services.Interfaces;
using Fan.Data;
using Fan.Medias;
using Fan.Settings;
using Fan.Shortcodes;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Threading.Tasks;

namespace Fan.Blog.UnitTests.Base
{
    /// <summary>
    /// Base class for <see cref="BlogPostService"/> unit tests.
    /// </summary>
    public class BlogServiceUnitTestBase
    {
        protected Mock<IPostRepository> _postRepoMock;
        protected Mock<IMetaRepository> _metaRepoMock;
        protected Mock<ICategoryRepository> _catRepoMock;
        protected Mock<ITagRepository> _tagRepoMock;
        protected BlogPostService _blogPostSvc; // we have test internal methods, thus not using IBlogPostService
        protected IDistributedCache _cache;
        protected ILogger<BlogPostService> _loggerBlogSvc;
        protected ILogger<SettingService> _loggerSettingSvc;

        protected Mock<ISettingService> _settingSvcMock;
        protected ICategoryService _catSvc;
        protected ILogger<CategoryService> _loggerCatSvc;
        protected ITagService _tagSvc;
        protected IImageService _imgSvc;
        protected Mock<IMediaService> _mediaSvcMock;
        protected ILogger<TagService> _loggerTagSvc;
        protected Mock<IStorageProvider> _storageProMock;

        /// <summary>
        /// Base constructor which will be called first for each test in derived test classes, thus
        /// setting up mocked repos and components here.
        /// </summary>
        public BlogServiceUnitTestBase()
        {
            // repos
            _postRepoMock = new Mock<IPostRepository>();
            _metaRepoMock = new Mock<IMetaRepository>();
            _catRepoMock = new Mock<ICategoryRepository>();
            _tagRepoMock = new Mock<ITagRepository>();

            // cache
            var serviceProvider = new ServiceCollection().AddMemoryCache().AddLogging().BuildServiceProvider();
            var memCacheOptions = serviceProvider.GetService<IOptions<MemoryDistributedCacheOptions>>();
            _cache = new MemoryDistributedCache(memCacheOptions);

            // logger
            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            _loggerBlogSvc = loggerFactory.CreateLogger<BlogPostService>();
            _loggerSettingSvc = loggerFactory.CreateLogger<SettingService>();

            // services (must be after _cache)
            _mediaSvcMock = new Mock<IMediaService>();

            // settings
            _settingSvcMock = new Mock<ISettingService>();
            _settingSvcMock.Setup(svc => svc.GetSettingsAsync<CoreSettings>()).Returns(Task.FromResult(new CoreSettings()));
            _settingSvcMock.Setup(svc => svc.GetSettingsAsync<BlogSettings>()).Returns(Task.FromResult(new BlogSettings()));

            // appsettings
            var appSettingsMock = new Mock<IOptionsSnapshot<AppSettings>>();
            appSettingsMock.Setup(o => o.Value).Returns(new AppSettings());

            // storage
            _storageProMock = new Mock<IStorageProvider>();

            // mapper, shortcode, mediator
            var mapper = BlogUtil.Mapper;
            var shortcodeSvc = new Mock<IShortcodeService>();
            var mediatorMock = new Mock<IMediator>();

            // cat service
            _loggerCatSvc = loggerFactory.CreateLogger<CategoryService>();
            _catSvc = new CategoryService(_catRepoMock.Object, _settingSvcMock.Object, mediatorMock.Object, _cache, _loggerCatSvc);

            // tag service
            _loggerTagSvc = loggerFactory.CreateLogger<TagService>();
            _tagSvc = new TagService(_tagRepoMock.Object, mediatorMock.Object, _cache, _loggerTagSvc);

            // image service
            _imgSvc = new ImageService(_mediaSvcMock.Object, _storageProMock.Object, appSettingsMock.Object);

            // post service
            _blogPostSvc = new BlogPostService(
                _settingSvcMock.Object,
                _imgSvc,
                _postRepoMock.Object,
                _cache,
                _loggerBlogSvc,
                mapper,
                shortcodeSvc.Object,
                mediatorMock.Object);
        }
    }
}

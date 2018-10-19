using Fan.Blog.Data;
using Fan.Blog.Helpers;
using Fan.Blog.Models;
using Fan.Blog.Services;
using Fan.Blog.Tags;
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

namespace Fan.Blog.IntegrationTests.Base
{
    /// <summary>
    /// Blog integration test base.
    /// </summary>
    public class BlogServiceIntegrationTestBase : BlogIntegrationTestBase
    {
        protected IBlogService _blogSvc;
        protected ITagService _tagSvc;
        protected Mock<ISettingService> _settingSvcMock;
        protected Mock<IMediaService> _mediaSvcMock;
        protected ILoggerFactory _loggerFactory;
        private readonly IMediaService _mediaSvc;
        protected Mock<IStorageProvider> _storageProviderMock;

        protected const string STORAGE_ENDPOINT = "https://www.fanray.com";

        public BlogServiceIntegrationTestBase()
        {
            // ---------------------------------------------------------------- repos

            var catRepo = new SqlCategoryRepository(_db);
            var tagRepo = new SqlTagRepository(_db);
            var postRepo = new SqlPostRepository(_db);

            // ---------------------------------------------------------------- mock SettingService 

            _settingSvcMock = new Mock<ISettingService>();
            _settingSvcMock.Setup(svc => svc.GetSettingsAsync<CoreSettings>()).Returns(Task.FromResult(new CoreSettings()));
            _settingSvcMock.Setup(svc => svc.GetSettingsAsync<BlogSettings>()).Returns(Task.FromResult(new BlogSettings()));

            // ---------------------------------------------------------------- mock AppSettings

            var appSettingsMock = new Mock<IOptionsSnapshot<AppSettings>>();
            appSettingsMock.Setup(o => o.Value).Returns(new AppSettings());

            // ---------------------------------------------------------------- mock IStorageProvider

            _storageProviderMock = new Mock<IStorageProvider>();
            _storageProviderMock.Setup(pro => pro.StorageEndpoint).Returns(STORAGE_ENDPOINT);

            // ---------------------------------------------------------------- MediaService

            var mediaRepo = new SqlMediaRepository(_db);
            _mediaSvc = new MediaService(_storageProviderMock.Object, appSettingsMock.Object, mediaRepo);

            // ---------------------------------------------------------------- Cache

            var serviceProvider = new ServiceCollection().AddMemoryCache().AddLogging().BuildServiceProvider();
            var memCacheOptions = serviceProvider.GetService<IOptions<MemoryDistributedCacheOptions>>();
            var cache = new MemoryDistributedCache(memCacheOptions);

            // ---------------------------------------------------------------- LoggerFactory

            _loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            var loggerBlogSvc = _loggerFactory.CreateLogger<BlogService>();
            var loggerTagSvc = _loggerFactory.CreateLogger<TagService>();

            // ---------------------------------------------------------------- Mapper, Shortcode

            var mapper = BlogUtil.Mapper;
            var shortcodeSvc = new Mock<IShortcodeService>();

            // ---------------------------------------------------------------- MediatR

            var services = new ServiceCollection();
            services.AddScoped<ServiceFactory>(p => p.GetService);  // MediatR.ServiceFactory
            services.AddSingleton<FanDbContext>(_db);               // DbContext for repos
            services.AddSingleton<IDistributedCache>(cache);
            services.AddSingleton<ILogger<TagService>>(loggerTagSvc);
            services.AddScoped<ITagRepository, SqlTagRepository>(); // will get _db

            services.Scan(scan => scan
               .FromAssembliesOf(typeof(IMediator), typeof(IBlogService))
               .AddClasses()
               .AsImplementedInterfaces());

            var provider = services.BuildServiceProvider();
            var mediator = provider.GetRequiredService<IMediator>();

            _tagSvc = new TagService(tagRepo, mediator, cache, loggerTagSvc);

            // the blog service
            _blogSvc = new BlogService(
                _settingSvcMock.Object,
                catRepo,
                postRepo,
                _mediaSvc,
                _storageProviderMock.Object,
                appSettingsMock.Object,
                cache,
                loggerBlogSvc,
                mapper,
                shortcodeSvc.Object,
                mediator);
        }
    }
}

using AutoMapper;
using Fan.Data;
using Fan.Helpers;
using Fan.Services;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Fan.Tests.Services
{
    /// <summary>
    /// Base class for <see cref="BlogService"/> tests.
    /// </summary>
    public class BlogServiceTest
    {
        protected Mock<IPostRepository> _postRepoMock;
        protected Mock<IMetaRepository> _metaRepoMock;
        protected Mock<ICategoryRepository> _catRepoMock;
        protected Mock<ITagRepository> _tagRepoMock;
        protected BlogService _blogSvc;
        protected IMapper _mapper;

        /// <summary>
        /// Base constructor which will be called first for each test in derived test classes, thus
        /// setting up mocked repos and components here.
        /// </summary>
        public BlogServiceTest()
        {
            // repos
            _postRepoMock = new Mock<IPostRepository>();
            _metaRepoMock = new Mock<IMetaRepository>();
            _catRepoMock = new Mock<ICategoryRepository>();
            _tagRepoMock = new Mock<ITagRepository>();

            // cache, loggerFactory, mapper
            var serviceProvider = new ServiceCollection().AddMemoryCache().AddLogging().BuildServiceProvider();
            var memCacheOptions = serviceProvider.GetService<IOptions<MemoryDistributedCacheOptions>>();
            var cache = new MemoryDistributedCache(memCacheOptions);
            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            _mapper = Config.Mapper;

            // svc
            _blogSvc = new BlogService(_catRepoMock.Object, _metaRepoMock.Object, _postRepoMock.Object, _tagRepoMock.Object,
                cache, loggerFactory, _mapper);
        }
    }
}

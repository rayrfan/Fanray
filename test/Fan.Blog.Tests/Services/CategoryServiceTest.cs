using Fan.Blog.Data;
using Fan.Blog.Helpers;
using Fan.Blog.Models;
using Fan.Blog.Services;
using Fan.Blog.Services.Interfaces;
using Fan.Exceptions;
using Fan.Settings;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Fan.Blog.Tests.Services
{
    public class CategoryServiceTest
    {
        private readonly ICategoryService categoryService;
        private readonly Mock<ICategoryRepository> catRepoMock = new Mock<ICategoryRepository>();
        private readonly Mock<IMediator> mediatorMock = new Mock<IMediator>();
        private readonly IDistributedCache cache;

        public CategoryServiceTest()
        {
            // cache, logger
            var serviceProvider = new ServiceCollection().AddMemoryCache().AddLogging().BuildServiceProvider();
            cache = new MemoryDistributedCache(serviceProvider.GetService<IOptions<MemoryDistributedCacheOptions>>());
            var logger = serviceProvider.GetService<ILoggerFactory>().CreateLogger<CategoryService>();

            // settings
            var settingSvcMock = new Mock<ISettingService>();
            settingSvcMock.Setup(svc => svc.GetSettingsAsync<CoreSettings>()).Returns(Task.FromResult(new CoreSettings()));
            settingSvcMock.Setup(svc => svc.GetSettingsAsync<BlogSettings>()).Returns(Task.FromResult(new BlogSettings()));

            // setup the default category in db
            var defaultCat = new Category { Id = 1, Title = "Web Development", Slug = "web-development" };
            catRepoMock.Setup(c => c.GetAsync(1)).Returns(Task.FromResult(defaultCat));
            catRepoMock.Setup(r => r.GetListAsync()).Returns(Task.FromResult(new List<Category> { defaultCat }));

            // cat service
            categoryService = new CategoryService(catRepoMock.Object, settingSvcMock.Object, mediatorMock.Object, cache, logger);
        }

        /// <summary>
        /// Creates a category with empty title will throw FanException.
        /// </summary>
        /// <param name="title"></param>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async void Create_category_with_empty_title_throws_FanException(string title)
        {
            await Assert.ThrowsAsync<FanException>(() => categoryService.CreateAsync(title));
        }

        /// <summary>
        /// Creates a category with a title that already exists throws FanException.
        /// </summary>
        [Fact]
        public async void Create_category_throws_FanException_if_title_already_exists()
        {
            try
            {
                var title = "web development";

                await categoryService.CreateAsync(title);
            }
            catch (FanException ex)
            {
                Assert.Equal("'web development' already exists.", ex.Message);
            }
        }

        /// <summary>
        /// Test <see cref="CategoryService.CreateCategoryAsync(Category)"/> would call Repository's 
        /// CreateAsync and invalidates cache for all categories.
        /// </summary>
        [Fact]
        public async void Create_category_calls_repo_and_invalidates_cache_for_all_categories()
        {
            // Arrange 
            var cat = new Category { Title = "Cat1" };

            // Act
            await categoryService.CreateAsync(cat.Title);

            // Assert
            catRepoMock.Verify(repo => repo.CreateAsync(It.IsAny<Category>()), Times.Exactly(1));
            Assert.Null(await cache.GetAsync(BlogCache.KEY_ALL_CATS));
        }

        /// <summary>
        /// Delete category calls repository and invalidates cache for all categories.
        /// </summary>
        /// <remarks>
        /// This test depends on Mock<ISettingService> to provide BlogSettings. 
        /// </remarks>
        [Fact]
        public async void Delete_category_calls_repo_and_invalidates_cache_for_all_categories()
        {
            // Act
            await categoryService.DeleteAsync(2);

            // Assert
            catRepoMock.Verify(repo => repo.DeleteAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Exactly(1));
            Assert.Null(await cache.GetAsync(BlogCache.KEY_ALL_CATS));
        }

        /// <summary>
        /// Update category would call CategoryRepository and then invalidates cache for all categories.
        /// </summary>
        [Fact]
        public async void Update_category_calls_repo_and_invalidates_cache_for_all_categories()
        {
            // Arrange
            var cat = await categoryService.GetAsync(1);

            // Act
            cat.Title = "Cat1";
            await categoryService.UpdateAsync(cat);

            // Assert
            catRepoMock.Verify(repo => repo.UpdateAsync(It.IsAny<Category>()), Times.Exactly(1));
            Assert.Null(await cache.GetAsync(BlogCache.KEY_ALL_CATS));
        }

        /// <summary>
        /// Category needs a valid id and title to update.
        /// </summary>
        [Fact]
        public async void Update_category_with_invalid_category_throws_FanException()
        {
            await Assert.ThrowsAsync<FanException>(() => categoryService.UpdateAsync(null));
            await Assert.ThrowsAsync<FanException>(() => categoryService.UpdateAsync(new Category())); // invalid id and title
            await Assert.ThrowsAsync<FanException>(() => categoryService.UpdateAsync(new Category { Id = 1 })); // invalid title
        }

        /// <summary>
        /// <see cref="ICategoryService.UpdateAsync(Tag)"/> treats title insensitively.
        /// </summary>
        [Fact]
        public async void Update_category_with_title_changed_only_in_casing_is_OK()
        {
            var cat = await categoryService.GetAsync(1);
            Assert.Equal("Web Development", cat.Title); 

            cat.Title = "web development";

            var catAgain = await categoryService.UpdateAsync(cat);
            Assert.Equal(1, catAgain.Id);
            Assert.Equal("web development", catAgain.Title);
        }
    }
}

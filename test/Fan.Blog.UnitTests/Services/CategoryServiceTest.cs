using Fan.Blog.Helpers;
using Fan.Blog.Models;
using Fan.Blog.UnitTests.Base;
using Fan.Exceptions;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Fan.Blog.UnitTests.Services
{
    /// <summary>
    /// Unit tests for <see cref="CategoryService"/>.
    /// </summary>
    public class CategoryServiceTest : BlogUnitTestBase
    {
        /// <summary>
        /// Given the default category.
        /// </summary>
        public CategoryServiceTest()
        {
            // setup the default category in db
            var defaultCat = new Category { Id = 1, Title = "Web Development", Slug = "web-development" };
            _catRepoMock.Setup(c => c.GetAsync(1)).Returns(Task.FromResult(defaultCat));
            _catRepoMock.Setup(r => r.GetListAsync()).Returns(Task.FromResult(new List<Category> { defaultCat }));
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
            await Assert.ThrowsAsync<FanException>(() => _catSvc.CreateAsync(title));
        }

        /// <summary>
        /// Creates a category with a title that already exists throws FanException.
        /// </summary>
        [Fact]
        public async void Create_category_throws_FanException_if_title_already_exists()
        {
            var title = "web development";
            await Assert.ThrowsAsync<FanException>(() => _catSvc.CreateAsync(title));

            try
            {
                await _catSvc.CreateAsync(title);
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
            await _catSvc.CreateAsync(cat.Title);

            // Assert
            _catRepoMock.Verify(repo => repo.CreateAsync(It.IsAny<Category>()), Times.Exactly(1));
            Assert.Null(await _cache.GetAsync(BlogCache.KEY_ALL_CATS));
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
            await _catSvc.DeleteAsync(2);

            // Assert
            _catRepoMock.Verify(repo => repo.DeleteAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Exactly(1));
            Assert.Null(await _cache.GetAsync(BlogCache.KEY_ALL_CATS));
        }

        /// <summary>
        /// Update category would call CategoryRepository and then invalidates cache for all categories.
        /// </summary>
        [Fact]
        public async void Update_category_calls_repo_and_invalidates_cache_for_all_categories()
        {
            // Arrange
            var cat = await _catSvc.GetAsync(1);

            // Act
            cat.Title = "Cat1";
            await _catSvc.UpdateAsync(cat);

            // Assert
            _catRepoMock.Verify(repo => repo.UpdateAsync(It.IsAny<Category>()), Times.Exactly(1));
            Assert.Null(await _cache.GetAsync(BlogCache.KEY_ALL_CATS));
        }

        /// <summary>
        /// Category needs a valid id and title to update.
        /// </summary>
        [Fact]
        public async void Update_category_with_invalid_category_throws_FanException()
        {
            await Assert.ThrowsAsync<FanException>(() => _catSvc.UpdateAsync(null));
            await Assert.ThrowsAsync<FanException>(() => _catSvc.UpdateAsync(new Category())); // invalid id and title
            await Assert.ThrowsAsync<FanException>(() => _catSvc.UpdateAsync(new Category { Id = 1 })); // invalid title
        }

        /// <summary>
        /// <see cref="ICategoryService.UpdateAsync(Tag)"/> treats title insensitively.
        /// </summary>
        [Fact]
        public async void Update_category_with_title_changed_only_in_casing_is_OK()
        {
            var cat = await _catSvc.GetAsync(1);
            Assert.Equal("Web Development", cat.Title); 

            cat.Title = "web development";

            var catAgain = await _catSvc.UpdateAsync(cat);
            Assert.Equal(1, catAgain.Id);
            Assert.Equal("web development", catAgain.Title);
        }
    }
}

using Fan.Blog.UnitTests.Base;
using Fan.Blog.Models;
using Fan.Blog.Services;
using Fan.Data;
using Fan.Exceptions;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Fan.Blog.UnitTests.Services
{
    /// <summary>
    /// Tests for <see cref="BlogService"/> category operations.
    /// </summary>
    public class CategoryTest : BlogServiceUnitTestBase
    {
        /// <summary>
        /// Given the default category.
        /// </summary>
        public CategoryTest()
        {
            // setup the default category in db
            _catRepoMock.Setup(r => r.GetListAsync()).Returns(Task.FromResult(
                new List<Category>
                {
                    new Category { Id = 1, Title = "Web Development", Slug = "web-development" }
                }
            ));
        }

        /// <summary>
        /// Test <see cref="BlogService.CreateCategoryAsync(Category)"/> would call Repository's CreateAsync and invalidates cache for all categories.
        /// </summary>
        [Fact]
        public async void Create_category_calls_repo_and_invalidates_cache_for_all_categories()
        {
            // Arrange 
            var cat = new Category { Title = "Cat1" };

            // Act
            await _blogSvc.CreateCategoryAsync(cat.Title);

            // Assert
            _catRepoMock.Verify(repo => repo.CreateAsync(It.IsAny<Category>()), Times.Exactly(1));
            Assert.Null(await _cache.GetAsync(BlogService.CACHE_KEY_ALL_CATS));
        }

        /// <summary>
        /// Delete category calls repository and invalidates cache for all categories.
        /// </summary>
        [Fact]
        public async void Delete_category_calls_repo_and_invalidates_cache_for_all_categories()
        {
            // Delete category requires access to default category id
            _metaRepoMock.Setup(repo => repo.GetAsync("blogsettings.defaultcategoryid"))
                .Returns(Task.FromResult(new Meta { Key = "blogsettings.defaultcategoryid", Value = "1" }));
            _metaRepoMock.Setup(repo => repo.AllAsync())
                .Returns(Task.FromResult(new List<Meta>() { new Meta() { Key = "blogsettings.defaultcategoryid", Value = "1" } }));

            // Act
            await _blogSvc.DeleteCategoryAsync(2);

            // Assert
            _catRepoMock.Verify(repo => repo.DeleteAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Exactly(1));
            Assert.Null(await _cache.GetAsync(BlogService.CACHE_KEY_ALL_CATS));
        }

        /// <summary>
        /// Update category would call CategoryRepository and then invalidates cache for all categories.
        /// </summary>
        [Fact]
        public async void Update_category_calls_repo_and_invalidates_cache_for_all_categories()
        {
            // Arrange
            var cat = new Category { Title = "Cat1" };

            // Act
            await _blogSvc.UpdateCategoryAsync(cat);

            // Assert
            _catRepoMock.Verify(repo => repo.UpdateAsync(It.IsAny<Category>()), Times.Exactly(1));
            Assert.Null(await _cache.GetAsync(BlogService.CACHE_KEY_ALL_CATS));
        }
    }
}

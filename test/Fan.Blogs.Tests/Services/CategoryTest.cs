using Fan.Blogs.Models;
using Fan.Blogs.Services;
using Fan.Exceptions;
using Fan.Models;
using Moq;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Fan.Blogs.Tests.Services
{
    /// <summary>
    /// Tests for <see cref="BlogService"/> category operations.
    /// </summary>
    public class CategoryTest : BlogUnitTestBase
    {
        /// <summary>
        /// Setting up existing categories.
        /// </summary>
        public CategoryTest()
        {
            // blog settings
            _metaRepoMock.Setup(repo => repo.GetAsync("BlogSettings"))
                .Returns(Task.FromResult(new Meta { Key = "BlogSettings", Value = JsonConvert.SerializeObject(new BlogSettings()) }));

            _catRepoMock.Setup(r => r.GetListAsync()).Returns(Task.FromResult(
                new List<Category>
                {
                    new Category { Title = "Technology", Slug = "tech" } 
                }
            ));
        }

        /// <summary>
        /// Test <see cref="BlogService.CreateCategoryAsync(Category)"/> with user input only title but no slug.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="expectSlug"></param>
        [Theory]
        [InlineData("Web Development!", "web-development")]
        [InlineData("C#", "cs")]
        [InlineData("Tech", "tech-2")]
        public async void CreateCategory_User_Input_Only_Title_No_Slug(string title, string expectSlug)
        {
            // Arrange: a category with a title no slug, and a repo to return it
            var category = new Category { Title = title };
            _catRepoMock.Setup(repo => repo.CreateAsync(It.IsAny<Category>())).Returns(Task.FromResult(category));

            // Act: when we create it
            category = await _blogSvc.CreateCategoryAsync(category);

            // Assert: then it has the expected slug
            Assert.Equal(expectSlug, category.Slug);
        }

        /// <summary>
        /// Test <see cref="BlogService.CreateCategoryAsync(Category)"/> with user input both title and slug.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="slug"></param>
        /// <param name="expectSlug"></param>
        [Theory]
        [InlineData("Web Development!", "whatever-i-want", "whatever-i-want")]
        [InlineData("Tech", "tech", "tech-2")]
        public async void CreateCategory_User_Input_Title_And_Slug(string title, string slug, string expectSlug)
        {
            // Arrange: a category and a repo to return it
            var category = new Category { Title = title, Slug = slug };
            _catRepoMock.Setup(repo => repo.CreateAsync(It.IsAny<Category>())).Returns(Task.FromResult(category));

            // Act: when we create it
            category = await _blogSvc.CreateCategoryAsync(category);

            // Assert: then it has the expected slug
            Assert.Equal(expectSlug, category.Slug);
        }

        /// <summary>
        /// Test <see cref="BlogService.CreateCategoryAsync(Category)"/> throws excpetion if title exists already.
        /// </summary>
        [Fact]
        public async void CreateCategory_Throws_FanException_If_Title_Already_Exist()
        {
            // Arrange: a category with a title that exists
            var category = new Category { Title = "Technology" };

            // Act and Assert: when we create it, we get exception
            await Assert.ThrowsAsync<FanException>(() => _blogSvc.CreateCategoryAsync(category));

            // Act and Assert: error message
            try
            {
                await _blogSvc.CreateCategoryAsync(category);
            }
            catch (FanException ex)
            {
                Assert.Equal("Failed to create Category.", ex.Message);
                Assert.Equal(1, ex.ValidationFailures.Count);
                Assert.Equal("Category 'Technology' is not available, please choose a different one.", ex.ValidationFailures[0].ErrorMessage);
            }
        }

        /// <summary>
        /// Test <see cref="BlogService.CreateCategoryAsync(Category)"/> a title in Chinese with no manual slug yields a 6-char random string.
        /// </summary>
        /// <remarks>
        /// User can name a category in any language, the algorithm for generating slug will give a 6-char slug.
        /// </remarks>
        [Fact]
        public async void CreateCategory_Chinese_Title_Without_Slug_Results_6_Char_Random_Slug()
        {
            // Arrange: a category with Chinese title no slug
            var category = new Category { Title = "你好" };
            _catRepoMock.Setup(repo => repo.CreateAsync(It.IsAny<Category>())).Returns(Task.FromResult(category));

            // Act: when we create it
            category = await _blogSvc.CreateCategoryAsync(category);

            // Assert: then it has the expected slug length
            Assert.Equal(6, category.Slug.Length);
        }

        /// <summary>
        /// Test <see cref="BlogService.CreateCategoryAsync(Category)"/> would call TagRepository's CreateAsync and invalidates cache for all categories.
        /// </summary>
        [Fact]
        public async void CreateCategory_Calls_TagRepository_CreateAsync_And_Invalidates_Cache_For_AllCategories()
        {
            // Arrange 
            var cat = new Category { Title = "Cat1" };

            // Act
            await _blogSvc.CreateCategoryAsync(cat);

            // Assert
            _catRepoMock.Verify(repo => repo.CreateAsync(It.IsAny<Category>()), Times.Exactly(1));
            Assert.Null(await _cache.GetAsync(BlogService.CACHE_KEY_ALL_CATS));
        }

        /// <summary>
        /// Test <see cref="BlogService.DeleteCategoryAsync(int)"/> cannot delete the default category.
        /// </summary>
        [Fact]
        public async void DeleteCategory_Cannot_Delete_Default_Category()
        {
            await Assert.ThrowsAsync<FanException>(() => _blogSvc.DeleteCategoryAsync(1));
        }

        /// <summary>
        /// Test <see cref="BlogService.DeleteCategoryAsync(int)"/> calls TagRepository's DeleteAsync and invalidates cache for all categories.
        /// </summary>
        [Fact]
        public async void DeleteCategory_Calls_TagRepository_DeleteAsync_And_Invalidates_Cache_For_AllCategories()
        {
            // Act
            await _blogSvc.DeleteCategoryAsync(2);

            // Assert
            _catRepoMock.Verify(repo => repo.DeleteAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Exactly(1));
            Assert.Null(await _cache.GetAsync(BlogService.CACHE_KEY_ALL_CATS));
        }

        /// <summary>
        /// Test <see cref="BlogService.GetCategoryAsync(string)"/> either by id or by slug throws exception if not found.
        /// </summary>
        [Fact]
        public async void GetCategory_Throws_FanException_If_Not_Found()
        {
            await Assert.ThrowsAsync<FanException>(() => _blogSvc.GetCategoryAsync(100));
            await Assert.ThrowsAsync<FanException>(() => _blogSvc.GetCategoryAsync("slug-not-exist"));
        }

        /// <summary>
        /// Test <see cref="BlogService.UpdateCategoryAsync(Category)"/> updates a category with new title and slug.
        /// </summary>
        /// <param name="newTitle"></param>
        /// <param name="newSlug"></param>
        /// <param name="expectedTitle"></param>
        /// <param name="expectedSlug"></param>
        [Theory]
        [InlineData("Web Development!", "whatever-i-want", "Web Development!", "whatever-i-want")]
        [InlineData("Tech", "tech", "Tech", "tech-2")]
        public async void UpdateCategory_Updates_New_Title_And_Slug(string newTitle, string newSlug, string expectedTitle, string expectedSlug)
        {
            // Arrange: a category
            var category = new Category { Title = newTitle, Slug = newSlug };
            _catRepoMock.Setup(repo => repo.UpdateAsync(It.IsAny<Category>())).Returns(Task.FromResult(category));

            // Act: update
            category = await _blogSvc.UpdateCategoryAsync(category);

            // Assert
            Assert.Equal(expectedTitle, category.Title);
            Assert.Equal(expectedSlug, category.Slug);
        }

        /// <summary>
        /// Test <see cref="BlogService.UpdateCategoryAsync(Category)"/> throws excpetion if title exists already.
        /// </summary>
        [Fact]
        public async void UpdateCategory_Throws_FanException_If_Title_Already_Exist()
        {
            // Arrange: a category with a title that exists
            var category = new Category { Title = "Technology" };

            // Act and Assert: when we create it, we get exception
            await Assert.ThrowsAsync<FanException>(() => _blogSvc.UpdateCategoryAsync(category));

            // Act and Assert: error message
            try
            {
                await _blogSvc.UpdateCategoryAsync(category);
            }
            catch (FanException ex)
            {
                Assert.Equal("Failed to update Category.", ex.Message);
                Assert.Equal(1, ex.ValidationFailures.Count);
                Assert.Equal("Category 'Technology' is not available, please choose a different one.", ex.ValidationFailures[0].ErrorMessage);
            }
        }

        /// <summary>
        /// Test <see cref="BlogService.UpdateCategoryAsync(Category)"/> would call CategoryRepository's UpdateAync and invalidates cache for all categories.
        /// </summary>
        [Fact]
        public async void UpdateCategory_Calls_CategoryRepository_And_Invalidates_Cache_For_All_Categories()
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

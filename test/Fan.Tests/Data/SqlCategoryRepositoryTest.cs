using Fan.Data;
using Fan.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using Xunit;

namespace Fan.Tests.Data
{
    /// <summary>
    /// Tests for <see cref="SqlCategoryRepository"/> class.
    /// </summary>
    public class SqlCategoryRepositoryTest : IDisposable
    {
        FanDbContext _db;
        SqlCategoryRepository _catRepo;

        public SqlCategoryRepositoryTest()
        {
            _db = DataTestHelper.GetContextWithSqlite();
            _catRepo = new SqlCategoryRepository(_db);
        }

        public void Dispose()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        /// <summary>
        /// Test for <see cref="SqlCategoryRepository.CreateAsync(Category)"/> method.
        /// </summary>
        [Fact]
        public async void CreateCategory_Creates_A_Category_In_Db()
        {
            // Arrange
            var cat = new Category { Slug = "cat-create", Title = "CategoryCreate" };

            // Act
            await _catRepo.CreateAsync(cat);

            // Assert
            Assert.NotNull(_db.Categories.SingleOrDefault(c => c.Title == "CategoryCreate"));
        }

        /// <summary>
        /// Test for <see cref="SqlCategoryRepository.DeleteAsync(int, int)"/> when a category is 
        /// deleted all posts belong to it will be assigned to a default category id.
        /// </summary>
        [Fact]
        public async void DeleteCategory_Will_Recategorize_Its_Posts_To_Default_Category()
        {
            // Arrange: given a post with a category
            _db.SeedTestPost();
            var existCat = _db.Categories.Single(c => c.Slug == DataTestHelper.CAT_SLUG);

            // Act: when we delete the existing category
            await _catRepo.DeleteAsync(existCat.Id, 1);

            // Assert: then post will have the default category
            var post = _db.Posts.Include(p => p.Category).Single(p => p.Slug == DataTestHelper.POST_SLUG);
            Assert.True(_db.Categories.Count() == 1);
            Assert.True(post.Category.Id == 1);
        }

        /// <summary>
        /// Test for <see cref="SqlCategoryRepository.GetListAsync"/> returns all the categories 
        /// in db.
        /// </summary>
        [Fact]
        public async void GetCategoryList_Returns_All_Categories()
        {
            // Arrange: given 2 categories
            var cat1 = new Category { Slug = "cat1", Title = "Category1" };
            var cat2 = new Category { Slug = "cat2", Title = "Category2" };
            await _catRepo.CreateAsync(cat1);
            await _catRepo.CreateAsync(cat2);

            // Act: when we call GetList
            var list = await _catRepo.GetListAsync();

            // Assert: then we get back all cats
            Assert.Equal(2, list.Count);
        }

        /// <summary>
        /// Test for <see cref="SqlCategoryRepository.GetListAsync"/> and the returned categories 
        /// are not tracked by EF.
        /// </summary>
        [Fact]
        public async void GetCategoryList_Returns_NonTracked_Categories()
        {
            // Arrange: create 2 categories
            var cat1 = new Category { Slug = "cat1", Title = "Category1" };
            var cat2 = new Category { Slug = "cat2", Title = "Category2" };
            await _catRepo.CreateAsync(cat1);
            await _catRepo.CreateAsync(cat2);

            // Act: when we call GetList
            var list = await _catRepo.GetListAsync();
            // get cat1 out from list and give it a new title
            var cat = list.FirstOrDefault(c => c.Slug == "cat1");
            cat.Title = "New Cat";
            // save
            await _catRepo.UpdateAsync(cat);

            // get cat1 out from db again
            var catAgain = _db.Categories.FirstOrDefault(c => c.Slug == "cat1");
            Assert.NotEqual("New Cat", catAgain.Title);
        }

        /// <summary>
        /// Test for <see cref="SqlCategoryRepository.UpdateAsync(Category)"/> method.
        /// </summary>
        [Fact]
        public async void UpdateCategory_Updates_It_In_Db()
        {
            // Arrange: given a cat
            var cat = new Category { Slug = "cat1", Title = "Category1" };
            await _catRepo.CreateAsync(cat);

            // Act: when we update its title
            var catAgain = _db.Categories.Single(c => c.Slug == "cat1");
            catAgain.Title = "Dog";
            catAgain.Slug = "dog";
            await _catRepo.UpdateAsync(catAgain);

            // This won't work! To update an entity it needs to be tracked first.
            //var catAgain = new Category { Id = 1, Slug = "dog", Title = "Dog" };
            //await _catRepo.UpdateAsync(catAgain);

            // Assert: then the category's title and slug are updated
            catAgain = _db.Categories.Single(c => c.Slug == "dog");
            Assert.Equal("Dog", catAgain.Title);
            Assert.Equal("dog", catAgain.Slug);
            Assert.Equal(1, catAgain.Id);
        }
    }
}

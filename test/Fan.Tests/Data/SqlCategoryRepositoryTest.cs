using Fan.Data;
using Fan.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using Xunit;

namespace Fan.Tests.Data
{
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
        /// Test for <see cref="SqlCategoryRepository.DeleteAsync(int, int)"/> method.
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
        /// Test for <see cref="SqlCategoryRepository.GetListAsync"/> method.
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
        /// Test for <see cref="SqlCategoryRepository.UpdateAsync(Category)"/> method.
        /// </summary>
        [Fact]
        public async void UpdateCategory_Updates_It_In_Db()
        {
            // Arrange: given a cat
            var cat1 = new Category { Slug = "cat1", Title = "Category1" };
            await _catRepo.CreateAsync(cat1);

            // Act: when we update its title
            var cat = _db.Categories.Single(c => c.Slug == "cat1");
            cat.Title = "Dog";
            await _catRepo.UpdateAsync(cat);

            /**
             * This won't work!
             * To update an entity it must be tracked first, if you just create it and update it, you'll get
             * System.InvalidOperationException: 'The instance of entity type 'Category' cannot be tracked because 
             * another instance of this type with the same key is already being tracked.
             * This means you must do a select before update!
             */
            //Category catAgain = new Category { Id = 1, Slug = "dog", Title = "Dog" };
            //_db.Update(catAgain);
            //_db.SaveChanges();

            // Assert: then the category's title is updated
            var catAgain = _db.Categories.Single(c => c.Slug == "cat1");
            Assert.Equal("Dog", catAgain.Title);
            Assert.Equal(1, catAgain.Id);
        }
    }
}

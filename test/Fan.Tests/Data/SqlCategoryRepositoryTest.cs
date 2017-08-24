using Fan.Data;
using Fan.Models;
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

        [Fact]
        public async void Category_Create_Test()
        {
            // Arrange
            var cat = new Category { Slug = "cat-create", Title = "CategoryCreate" };

            // Act
            await _catRepo.CreateAsync(cat);

            // Assert
            Assert.NotNull(_db.Categories.SingleOrDefault(c => c.Title == "CategoryCreate"));
        }
    }
}

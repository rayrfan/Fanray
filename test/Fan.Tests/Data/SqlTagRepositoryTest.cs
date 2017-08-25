using Fan.Data;
using Fan.Models;
using System;
using System.Linq;
using Xunit;

namespace Fan.Tests.Data
{
    /// <summary>
    /// Tests for <see cref="SqlPostRepository"/> class.
    /// </summary>
    public class SqlTagRepositoryTest : IDisposable
    {
        FanDbContext _db;
        SqlTagRepository _tagRepo;
        public SqlTagRepositoryTest()
        {
            _db = DataTestHelper.GetContextWithSqlite();
            _tagRepo = new SqlTagRepository(_db);
        }

        public void Dispose()
        {
            _db.Database.EnsureDeleted(); 
            _db.Dispose();
        }

        /// <summary>
        /// Test for <see cref="SqlTagRepository.CreateAsync(Tag)"/> method.
        /// </summary>
        [Fact]
        public async void CreateTag_Creates_A_Tag_In_Db()
        {
            // Arrange
            var tag = new Tag { Slug = "tag", Title = "Tag" };

            // Act
            await _tagRepo.CreateAsync(tag);

            // Assert
            Assert.NotNull(_db.Tags.SingleOrDefault(c => c.Title == "Tag"));
        }

        /// <summary>
        /// Test for <see cref="SqlTagRepository.DeleteAsync(int)"/>, when deleting a tag will
        /// automatically delete the associations, because Tag and PostTag are related by FK 
        /// with cascade delete.
        /// </summary>
        [Fact]
        public async void DeleteTag_Will_Delete_PostTag_Association_By_Cascade_Delete()
        {
            // Arrange
            _db.SeedTestPost();

            // Act
            await _tagRepo.DeleteAsync(1);

            // Assert
            Assert.True(_db.PostTags.Count() == 1);
        }

        /// <summary>
        /// Test for <see cref="SqlTagRepository.GetListAsync"/> returns PostCount with each
        /// tag.
        /// </summary>
        [Fact]
        public async void GetTagList_Returns_PostCount_With_Tags()
        {
            // Arrange: tag2 are all labeled on drafts
            _db.SeedTestPosts(11);

            // Act
            var list = await _tagRepo.GetListAsync();

            // Assert: therefore tag2 count is 0
            Assert.Equal(2, list.Count);
            Assert.Equal(0, list[1].Count);
        }

        [Fact]
        public async void UpdateTag_Updates_It_In_Db()
        {
            // Arrange: given a tag
            var tag = new Tag { Slug = "tag", Title = "Tag" };
            await _tagRepo.CreateAsync(tag);

            // Act: when we update its title
            var tagAgain = _db.Tags.Single(t => t.Slug == "tag");
            tagAgain.Title = "Tag2";
            tagAgain.Slug = "tag2";
            await _tagRepo.UpdateAsync(tagAgain);

            // Assert: then the tag's title and slug are updated
            var catAgain = _db.Tags.Single(c => c.Slug == "tag2");
            Assert.Equal("Tag2", catAgain.Title);
            Assert.Equal("tag2", catAgain.Slug);
            Assert.Equal(1, catAgain.Id);
        }
    }
}

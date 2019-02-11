using Fan.Data;
using Fan.IntegrationTests.Base;
using System.Linq;
using Xunit;

namespace Fan.IntegrationTests.Data
{
    /// <summary>
    /// Tests for <see cref="SqlMetaRepository"/> class.
    /// </summary>
    public class SqlMetaRepositoryTest : IntegrationTestBase
    {
        private SqlMetaRepository _repo;

        public SqlMetaRepositoryTest()
        {
            _repo = new SqlMetaRepository(_db);
        }

        /// <summary>
        /// Test for <see cref="SqlMetaRepository.CreateAsync(Meta)"/> method when key already exists.
        /// </summary>
        [Fact]
        public async void Meta_table_allows_duplicate_keys()
        {
            // Arrange
            var meta = new Meta { Key = "key", Value = "value" };
            var meta2 = new Meta { Key = "key", Value = "value" };

            // Act & Assert
            await _repo.CreateAsync(meta);
            await _repo.CreateAsync(meta2);
        }

        /// <summary>
        /// Test for <see cref="SqlMetaRepository.CreateAsync(Setting)"/> method will create a new 
        /// record in the Meta table.
        /// </summary>
        [Fact]
        public async void Create_meta_saves_a_record_in_Core_Setting_table()
        {
            // Arrange
            var meta = new Meta { Key = "Age", Value = "13" };

            // Act
            await _repo.CreateAsync(meta);

            // Assert
            var metaAgain = _db.Set<Meta>().SingleOrDefault(c => c.Key == "Age");
            Assert.Equal("13", metaAgain.Value);
        }

        /// <summary>
        /// Test for <see cref="SqlMetaRepository.GetAsync(string)"/> method will return null if 
        /// the key is not found.
        /// </summary>
        [Fact]
        public async void Get_meta_returns_null_if_key_does_not_exist()
        {
            // Act
            var meta = await _repo.GetAsync("NotExist");

            // Assert
            Assert.Null(meta);
        }

        /// <summary>
        /// Test for <see cref="SqlMetaRepository.GetAsync(string)"/> will return the record 
        /// specified by the key if it's in db.
        /// </summary>
        [Fact]
        public async void Get_meta_returns_the_record()
        {
            // Arrange
            await _repo.CreateAsync(new Meta { Key = "Age", Value = "13" });

            // Act
            var meta = await _repo.GetAsync("Age");

            // Assert
            Assert.Equal("Age", meta.Key);
        }

        /// <summary>
        /// Test for <see cref="SqlMetaRepository.UpdateAsync(Meta)"/> method.
        /// </summary>
        [Fact]
        public async void Update_meta_updates_it_in_db()
        {
            // Arrange
            var meta = await _repo.CreateAsync(new Meta { Key = "key", Value = "value" });

            // Act
            meta.Value = "new value";
            await _repo.UpdateAsync(meta);

            // Assert
            var metaAgain = await _repo.GetAsync("key");
            Assert.Equal("new value", metaAgain.Value);
        }

        /// <summary>
        /// Test for <see cref="SqlMetaRepository.UpdateAsync(Meta)"/> method.
        /// </summary>
        [Fact]
        public async void Update_meta_updates_a_record_with_key_not_found_does_nothing()
        {
            // Arrange
            var meta = new Meta { Key = "key-not-found", Value = "value" };

            // Act
            await _repo.UpdateAsync(meta);

            // Assert
            var metaAgain = await _repo.GetAsync("key-not-found");
            Assert.Null(metaAgain);
        }
    }
}

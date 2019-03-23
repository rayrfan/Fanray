using Fan.Data;
using Fan.Exceptions;
using Fan.IntegrationTests.Base;
using System.Linq;
using Xunit;

namespace Fan.IntegrationTests.Data
{
    /// <summary>
    /// Tests for <see cref="SqlMetaRepository"/> class.
    /// </summary>
    /// <remarks>
    /// Meta table records must be unique by Key + Type columns. 
    /// When inserting Key can be any casing.
    /// When searching by Key casing is ignored.
    /// </remarks>
    public class SqlMetaRepositoryTest : IntegrationTestBase
    {
        private SqlMetaRepository _repo;

        public SqlMetaRepositoryTest()
        {
            _repo = new SqlMetaRepository(_db);
        }

        /// <summary>
        /// Test for <see cref="SqlMetaRepository.CreateAsync(Meta)"/> method when key and type already exists,
        /// <see cref="FanException"/> will be thrown. Create_meta_throws_FanException_if_key_and_type_already_exists
        /// </summary>
        [Fact]
        public async void Meta_table_does_not_allow_duplicate_key_type_pair()
        {
            // Given 2 meta records with same Key and same Type
            var meta = new Meta { Key = "key", Value = "value", Type = EMetaType.Setting };
            var meta2 = new Meta { Key = "key", Value = "value", Type = EMetaType.Setting };

            // When inserting the 2nd record, FanException will throw
            await _repo.CreateAsync(meta);
            var ex = await Assert.ThrowsAsync<FanException>(() => _repo.CreateAsync(meta2));
        }

        /// <summary>
        /// Meta table Key with value that is different in casing is considered different values.
        /// </summary>
        [Fact]
        public async void Meta_table_key_is_case_sensitive()
        {
            // Given 2 meta records with same Key different casing
            var meta = new Meta { Key = "key", Value = "value", Type = EMetaType.Setting };
            var meta2 = new Meta { Key = "Key", Value = "value", Type = EMetaType.Setting };

            // When inserting the 2nd record, FanException will NOT throw
            await _repo.CreateAsync(meta);
            await _repo.CreateAsync(meta2);
        }

        /// <summary>
        /// The Meta table's unique index is on Key and Type, so if you insert the same Key but with
        /// different Type, no exception will throw.
        /// </summary>
        [Fact]
        public async void Create_meta_does_not_throw_exception_if_type_is_different()
        {
            // Given 2 meta records with same Key but different Type
            var meta = new Meta { Key = "key", Value = "value", Type = EMetaType.Setting };
            var meta2 = new Meta { Key = "key", Value = "value", Type = EMetaType.Widget };

            // When insert both record
            var metaAgain = await _repo.CreateAsync(meta);
            var meta2Again = await _repo.CreateAsync(meta2);

            // Then no exception is thrown
            Assert.Equal(2, meta2Again.Id);
        }

        /// <summary>
        /// Test for <see cref="SqlMetaRepository.CreateAsync(Setting)"/> method will create a new 
        /// record in the Meta table.
        /// </summary>
        [Fact]
        public async void Create_meta_saves_a_record_in_Core_Setting_table()
        {
            // Arrange
            var meta = new Meta { Key = "Age", Value = "13", Type = EMetaType.Setting };

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
            var meta = await _repo.GetAsync("NotExist", EMetaType.Setting);

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
            await _repo.CreateAsync(new Meta { Key = "Age", Value = "13", Type = EMetaType.Setting });

            // Act
            var meta = await _repo.GetAsync("Age", EMetaType.Setting);

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
            var meta = await _repo.CreateAsync(new Meta { Key = "key", Value = "value", Type = EMetaType.Setting });

            // Act
            meta.Value = "new value";
            await _repo.UpdateAsync(meta);

            // Assert
            var metaAgain = await _repo.GetAsync("key", EMetaType.Setting);
            Assert.Equal("new value", metaAgain.Value);
        }

        /// <summary>
        /// Test for <see cref="SqlMetaRepository.UpdateAsync(Meta)"/> method.
        /// </summary>
        [Fact]
        public async void Update_meta_updates_a_record_with_key_not_found_does_nothing()
        {
            // Arrange
            var meta = new Meta { Key = "key-not-found", Value = "value", Type = EMetaType.Setting };

            // Act
            await _repo.UpdateAsync(meta);

            // Assert
            var metaAgain = await _repo.GetAsync("key-not-found", EMetaType.Setting);
            Assert.Null(metaAgain);
        }
    }
}

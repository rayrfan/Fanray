using Fan.Data;
using Fan.Exceptions;
using Fan.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Linq;
using Xunit;

namespace Fan.Tests.Data
{
    /// <summary>
    /// Tests for <see cref="SqlMetaRepository"/> class.
    /// </summary>
    public class SqlMetaRepositoryTest : DataTestBase
    {
        private SqlMetaRepository _metaRepo;

        public SqlMetaRepositoryTest()
        {
            _metaRepo = new SqlMetaRepository(_db, _loggerFactory.CreateLogger<SqlMetaRepository>());
        }

        /// <summary>
        /// Test for <see cref="SqlMetaRepository.CreateAsync(Meta)"/> method when key already exists,
        /// <see cref="FanException"/> will be thrown.
        /// </summary>
        [Fact]
        public async void CreateMeta_Throws_FanException_If_Key_Already_Exists()
        {
            // Arrange
            var meta = new Meta { Key = "key", Value = "value" };
            var meta2 = new Meta { Key = "key", Value = "value" };

            // Act
            await _metaRepo.CreateAsync(meta);
            var ex = await Assert.ThrowsAsync<FanException>(() => _metaRepo.CreateAsync(meta2));

            // Assert
            Assert.IsType<FanException>(ex);
        }

        /// <summary>
        /// Test for <see cref="SqlMetaRepository.CreateAsync(Meta)"/> method will create a new 
        /// record in the Meta table.
        /// </summary>
        [Fact]
        public async void CreateMeta_Creates_A_Meta_In_Db()
        {
            // Arrange
            var meta = new Meta { Key = "BlogSettings", Value = JsonConvert.SerializeObject(new BlogSettings()) };

            // Act
            await _metaRepo.CreateAsync(meta);

            // Assert
            Assert.NotNull(_db.Metas.SingleOrDefault(c => c.Key == "BlogSettings"));
        }

        /// <summary>
        /// Test for <see cref="SqlMetaRepository.GetAsync(string)"/> method will return null if 
        /// the key is not found.
        /// </summary>
        [Fact]
        public async void GetMeta_Returns_Null_If_Key_Does_Not_Exist()
        {
            // Act
            var meta = await _metaRepo.GetAsync("NotExist");

            // Assert
            Assert.Null(meta);
        }

        /// <summary>
        /// Test for <see cref="SqlMetaRepository.GetAsync(string)"/> will return the record 
        /// specified by the key if it's in db.
        /// </summary>
        [Fact]
        public async void GetMeta_Returns_The_Meta_With_Specified_Key()
        {
            // Arrange
            SeedTestPost();

            // Act
            var meta = await _metaRepo.GetAsync("BlogSettings");

            // Assert
            Assert.NotNull(meta);
            Assert.Equal("BlogSettings", meta.Key);
        }

        /// <summary>
        /// Test for <see cref="SqlMetaRepository.UpdateAsync(Meta)"/> method.
        /// </summary>
        [Fact]
        public async void UpdateMeta_Updates_It_In_Db()
        {
            // Arrange
            var meta = await _metaRepo.CreateAsync(new Meta { Key = "key", Value = "value" });

            // Act
            meta.Value = "new value";
            await _metaRepo.UpdateAsync(meta);

            // Assert
            var metaAgain = await _metaRepo.GetAsync("key");
            Assert.Equal("new value", meta.Value);
        }

        /// <summary>
        /// Test for <see cref="SqlMetaRepository.UpdateAsync(Meta)"/> method.
        /// </summary>
        [Fact]
        public async void UpdateMeta_Updates_A_Record_With_Key_NotFound_Does_Nothing()
        {
            // Arrange
            var meta = new Meta { Key = "key-not-found", Value = "value" };

            // Act
            await _metaRepo.UpdateAsync(meta);

            // Assert
            var metaAgain = await _metaRepo.GetAsync("key-not-found");
            Assert.Null(metaAgain);
        }
    }
}

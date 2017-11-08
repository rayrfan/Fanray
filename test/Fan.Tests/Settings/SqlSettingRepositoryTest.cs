using Fan.Data;
using Fan.Models;
using Fan.Settings;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Linq;
using Xunit;

namespace Fan.Tests.Data
{
    /// <summary>
    /// Tests for <see cref="SqlSettingRepository"/> class.
    /// </summary>
    public class SqlSettingRepositoryTest : DataTestBase
    {
        private SqlSettingRepository _repo;

        public SqlSettingRepositoryTest()
        {
            _repo = new SqlSettingRepository(_db);
        }

        /// <summary>
        /// Test for <see cref="SqlSettingRepository.CreateAsync(Meta)"/> method when key already exists,
        /// <see cref="DbUpdateException"/> will be thrown.
        /// </summary>
        [Fact]
        public async void Create_setting_throws_DbUpdateException_if_key_already_exists()
        {
            // Arrange
            var meta = new Setting { Key = "key", Value = "value" };
            var meta2 = new Setting { Key = "key", Value = "value" };

            // Act & Assert
            await _repo.CreateAsync(meta);
            var ex = await Assert.ThrowsAsync<DbUpdateException>(() => _repo.CreateAsync(meta2));
        }

        /// <summary>
        /// Test for <see cref="SqlSettingRepository.CreateAsync(Setting)"/> method will create a new 
        /// record in the Meta table.
        /// </summary>
        [Fact]
        public async void Create_setting_saves_a_record_in_Core_Setting_table()
        {
            // Arrange
            var setting = new Setting { Key = "Age", Value = "13" };

            // Act
            await _repo.CreateAsync(setting);

            // Assert
            var settingAgain = _db.Set<Setting>().SingleOrDefault(c => c.Key == "Age");
            Assert.Equal("13", settingAgain.Value);
        }

        /// <summary>
        /// Test for <see cref="SqlSettingRepository.GetAsync(string)"/> method will return null if 
        /// the key is not found.
        /// </summary>
        [Fact]
        public async void Get_setting_returns_null_if_key_does_not_exist()
        {
            // Act
            var setting = await _repo.GetAsync("NotExist");

            // Assert
            Assert.Null(setting);
        }

        /// <summary>
        /// Test for <see cref="SqlSettingRepository.GetAsync(string)"/> will return the record 
        /// specified by the key if it's in db.
        /// </summary>
        [Fact]
        public async void Get_setting_returns_the_record()
        {
            // Arrange
            await _repo.CreateAsync(new Setting { Key = "Age", Value = "13" });

            // Act
            var setting = await _repo.GetAsync("Age");

            // Assert
            Assert.Equal("Age", setting.Key);
        }

        /// <summary>
        /// Test for <see cref="SqlSettingRepository.UpdateAsync(Meta)"/> method.
        /// </summary>
        [Fact]
        public async void Update_setting_updates_it_in_db()
        {
            // Arrange
            var setting = await _repo.CreateAsync(new Setting { Key = "key", Value = "value" });

            // Act
            setting.Value = "new value";
            await _repo.UpdateAsync(setting);

            // Assert
            var settingAgain = await _repo.GetAsync("key");
            Assert.Equal("new value", settingAgain.Value);
        }

        /// <summary>
        /// Test for <see cref="SqlSettingRepository.UpdateAsync(Meta)"/> method.
        /// </summary>
        [Fact]
        public async void Update_setting_updates_a_record_with_key_not_found_does_nothing()
        {
            // Arrange
            var setting = new Setting { Key = "key-not-found", Value = "value" };

            // Act
            await _repo.UpdateAsync(setting);

            // Assert
            var settingAgain = await _repo.GetAsync("key-not-found");
            Assert.Null(settingAgain);
        }
    }
}

using Fan.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;

namespace Fan.Tests.Data
{
    public class DataTestBase : IDisposable
    {
        /// <summary>
        /// A <see cref="CoreDbContext"/> built with Sqlite in-memory mode.
        /// </summary>
        protected CoreDbContext _db;

        public DataTestBase()
        {
            _db = GetContextWithSqlite(); 
        }

        public void Dispose()
        {
            _db.Database.EnsureDeleted(); // important, otherwise SeedTestData is not erased
            _db.Dispose();
        }

        /// <summary>
        /// Returns <see cref="CoreDbContext"/> with SQLite Database Provider in-memory mode.
        /// </summary>
        private CoreDbContext GetContextWithSqlite()
        {
            var connection = new SqliteConnection() { ConnectionString = "Data Source=:memory:" };
            connection.Open();

            var builder = new DbContextOptionsBuilder<CoreDbContext>();
            builder.UseSqlite(connection);

            var context = new CoreDbContext(builder.Options);
            context.Database.EnsureCreated();

            return context;
        }

        /// <summary>
        /// Returns <see cref="CoreDbContext"/> with Entity Framework Core In-Memory Database.
        /// </summary>
        private CoreDbContext GetContextWithEFCore()
        {
            var _options = new DbContextOptionsBuilder<CoreDbContext>().UseInMemoryDatabase("FanInMemDb").Options;
            return new CoreDbContext(_options);
        }
    }
}

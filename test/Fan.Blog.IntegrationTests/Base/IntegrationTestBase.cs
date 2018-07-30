using Fan.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace Fan.Blog.IntegrationTests.Base
{
    public class IntegrationTestBase : IDisposable
    {
        /// <summary>
        /// A <see cref="FanDbContext"/> built with Sqlite in-memory mode.
        /// </summary>
        protected FanDbContext _db;
        private ILoggerFactory _loggerFactory;

        public IntegrationTestBase()
        {
            _loggerFactory = new ServiceCollection().AddLogging().BuildServiceProvider().GetService<ILoggerFactory>();
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
        private FanDbContext GetContextWithSqlite()
        {
            var connection = new SqliteConnection() { ConnectionString = "Data Source=:memory:" };
            connection.Open();

            var builder = new DbContextOptionsBuilder<FanDbContext>();
            builder.UseSqlite(connection);

            var context = new FanDbContext(builder.Options, _loggerFactory);
            context.Database.EnsureCreated();

            return context;
        }

        /// <summary>
        /// Returns <see cref="CoreDbContext"/> with Entity Framework Core In-Memory Database.
        /// </summary>
        private FanDbContext GetContextWithEFCore()
        {
            var _options = new DbContextOptionsBuilder<FanDbContext>().UseInMemoryDatabase("FanInMemDb").Options;
            return new FanDbContext(_options, _loggerFactory);
        }
    }
}

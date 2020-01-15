using Fan.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;

namespace Fan.Tests.Data
{
    /// <summary>
    /// Base class for all integration tests, it provides the <see cref="FanDbContext"/> with 
    /// in-memory database.
    /// </summary>
    /// <remarks>
    /// When it comes to test with an in-memory database, there are two choices, the 
    /// EF Core In-Memory Database Provider (Microsoft.EntityFrameworkCore.InMemory)
    /// or the SQLite Database Provider (Microsoft.EntityFrameworkCore.Sqlite) with the SQLite 
    /// in-memory mode. However EF Core provider does not enforce any integrity like a relational 
    /// database, for example, the Meta table cannot have duplicate keys, it doesn't enforce that.
    /// 
    /// For more info https://docs.microsoft.com/en-us/ef/core/miscellaneous/testing/index
    /// </remarks>
    public class IntegrationTestBase : IDisposable
    {
        /// <summary>
        /// A <see cref="FanDbContext"/> built with Sqlite in-memory mode.
        /// </summary>
        protected readonly FanDbContext _db;
        protected readonly ILoggerFactory loggerFactory = LoggerFactory.Create(builder => { builder.AddConsole(); });

        /// <summary>
        /// Initializes DbContext with SQLite Database Provider in-memory mode with logging to
        /// console and ensure database created.
        /// </summary>
        public IntegrationTestBase()
        {
            var connection = new SqliteConnection() { ConnectionString = "Data Source=:memory:" };
            connection.Open();

            var options = new DbContextOptionsBuilder<FanDbContext>()
                .UseLoggerFactory(loggerFactory)
                .UseSqlite(connection).Options;

            _db = new FanDbContext(options, loggerFactory);
            _db.Database.EnsureCreated();
        }

        public void Dispose()
        {
            _db.Database.EnsureDeleted(); // important, otherwise SeedTestData is not erased
            _db.Dispose();
        }
    }
}

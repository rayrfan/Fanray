using Fan.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Fan.Tests.Data
{
    public static class DataTestHelper
    {
        /// <summary>
        /// Returns <see cref="FanDbContext"/> with Entity Framework Core In-Memory Database.
        /// </summary>
        /// <returns></returns>
        public static FanDbContext GetContextWithEF()
        {
            var _options = new DbContextOptionsBuilder<FanDbContext>().UseInMemoryDatabase(databaseName: "Add_writes_to_database").Options;
            return new FanDbContext(_options);
        }

        /// <summary>
        /// Returns <see cref="FanDbContext"/> with SQLite Database Provider in-memory mode.
        /// </summary>
        /// <returns></returns>
        public static FanDbContext GetContextWithSqlite()
        {
            var context = new FanDbContext(GetSqliteOptions<FanDbContext>());
            context.Database.EnsureCreated();
            return context;
        }

        private static DbContextOptions<T> GetSqliteOptions<T>() where T : DbContext
        {
            var connection = new SqliteConnection()
            {
                ConnectionString = "Data Source=:memory:" 
            };
            connection.Open(); // https://github.com/aspnet/EntityFramework/issues/6968

            // create in-memory context
            var builder = new DbContextOptionsBuilder<T>();
            builder.UseSqlite(connection);

            return builder.Options;
        }
    }
}

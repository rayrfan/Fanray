using Fan.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Fan.Tests.Data
{
    public class DataTestHelper
    {
        /// <summary>
        /// Returns <see cref="FanDbContext"/> with SQLite Database Provider in-memory mode.
        /// </summary>
        /// <returns></returns>
        public static FanDbContext GetContextWithSqlite()
        {
            var connection = new SqliteConnection() { ConnectionString = "Data Source=:memory:" };
            connection.Open(); 

            var builder = new DbContextOptionsBuilder<FanDbContext>();
            builder.UseSqlite(connection);

            var context = new FanDbContext(builder.Options);
            context.Database.EnsureCreated();

            return context;
        }

        /// <summary>
        /// Returns <see cref="FanDbContext"/> with Entity Framework Core In-Memory Database.
        /// </summary>
        /// <returns></returns>
        public static FanDbContext GetContextWithEFCore()
        {
            var _options = new DbContextOptionsBuilder<FanDbContext>().UseInMemoryDatabase("FanInMemDb").Options;
            return new FanDbContext(_options);
        }
    }
}

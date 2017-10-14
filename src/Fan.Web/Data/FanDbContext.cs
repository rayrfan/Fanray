using Fan.Blogs.Data;
using Fan.Data;
using Fan.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Fan.Web.Data
{
    /// <summary>
    /// DbContext for entire database, used for initial db creation.
    /// </summary>
    /// <remarks>
    /// Having multiple DbContextes and still create database for user on app launch is a challenge,
    /// having a context for the entire db is a solution for now. 
    /// <see cref="https://stackoverflow.com/a/11198345/32240"/>
    /// </remarks>
    public class FanDbContext : IdentityDbContext<User, Role, int>
    {
        public FanDbContext(DbContextOptions<FanDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            CoreDbContext.CreateCoreModel(builder);
            BlogDbContext.CreateBlogModel(builder);
        }
    }
}

using Fan.Blogs.Models;
using Fan.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Fan.Blogs.Data
{
    public class BlogDbContext : IdentityDbContext<User, Role, int>
    {
        public virtual DbSet<Post> Posts { get; set; }
        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<Tag> Tags { get; set; }
        public virtual DbSet<PostTag> PostTags { get; set; }

        public BlogDbContext(DbContextOptions<BlogDbContext> options) 
            : base(options)
        {
        }
      
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<User>().ToTable("Core_User"); // post joins user
            CreateBlogModel(builder);
        }

        /// <summary>
        /// Blog specific models.
        /// </summary>
        /// <param name="builder"></param>
        public static void CreateBlogModel(ModelBuilder builder)
        {
            builder.Entity<Post>(entity =>
            {
                entity.ToTable("Blog_Post");
                entity.HasIndex(e => e.Slug);
                entity.HasIndex(e => new { e.Type, e.Status, e.CreatedOn, e.Id });
                entity.HasIndex(e => e.ParentId);
                entity.HasIndex(e => e.UserId);
            });

            builder.Entity<Category>(entity =>
            {
                entity.ToTable("Blog_Category");
                entity.HasKey(e => e.Id).ForSqlServerIsClustered(clustered: false);
                entity.HasIndex(e => e.Slug).IsUnique().ForSqlServerIsClustered();
            });

            builder.Entity<Tag>(entity =>
            {
                entity.ToTable("Blog_Tag");
                entity.HasKey(e => e.Id).ForSqlServerIsClustered(clustered: false);
                entity.HasIndex(e => e.Slug).IsUnique().ForSqlServerIsClustered();
            });

            builder.Entity<PostTag>(entity =>
            {
                entity.ToTable("Blog_PostTag");
                entity.HasKey(e => new { e.PostId, e.TagId });
            });
        }
    }
}

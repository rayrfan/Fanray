using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Fan.Models;

namespace Fan.Data
{
    public class FanDbContext : IdentityDbContext<User>
    {
        // If you want to prefix your tables
        private const string TABLE_PREFIX = "";

        public virtual DbSet<Meta> Metas { get; set; }
        public virtual DbSet<Post> Posts { get; set; }
        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<Tag> Tags { get; set; }
        public virtual DbSet<PostTag> PostTags { get; set; }

        public FanDbContext(DbContextOptions<FanDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // To create a schema, sqlserver only
            //builder.HasDefaultSchema("");

            builder.Entity<Meta>(entity =>
            {
                entity.ToTable($"{TABLE_PREFIX}Meta" /* , schema: "" */);
                entity.HasKey(e => e.Id).ForSqlServerIsClustered(clustered: false);
                entity.HasIndex(e => e.Key).IsUnique().ForSqlServerIsClustered();
            });

            builder.Entity<Post>(entity =>
            {
                entity.ToTable($"{TABLE_PREFIX}Post");
                entity.HasIndex(e => e.Slug);
                entity.HasIndex(e => new { e.Type, e.Status, e.CreatedOn, e.Id });
                entity.HasIndex(e => e.ParentId);
                entity.HasIndex(e => e.UserName);
            });

            builder.Entity<Category>(entity =>
            {
                entity.ToTable($"{TABLE_PREFIX}Category");
                entity.HasKey(e => e.Id).ForSqlServerIsClustered(clustered: false);
                entity.HasIndex(e => e.Slug).IsUnique().ForSqlServerIsClustered();
            });

            builder.Entity<Tag>(entity =>
            {
                entity.ToTable($"{TABLE_PREFIX}Tag");
                entity.HasKey(e => e.Id).ForSqlServerIsClustered(clustered: false);
                entity.HasIndex(e => e.Slug).IsUnique().ForSqlServerIsClustered();
            });

            builder.Entity<PostTag>(entity =>
            {
                entity.ToTable($"{TABLE_PREFIX}PostTag");
                entity.HasKey(e => new { e.PostId, e.TagId });
            });
        }
    }
}

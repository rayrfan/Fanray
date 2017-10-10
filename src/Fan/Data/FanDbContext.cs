using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Fan.Models;
using Microsoft.AspNetCore.Identity;

namespace Fan.Data
{
    public class FanDbContext : IdentityDbContext<User, Role, int>
    {
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

            builder.Entity<User>().ToTable("Fan_User");
            builder.Entity<Role>().ToTable("Fan_Role");
            builder.Entity<IdentityUserClaim<int>>().ToTable("Fan_UserClaim");
            builder.Entity<IdentityRoleClaim<int>>().ToTable("Fan_RoleClaim");
            builder.Entity<IdentityUserRole<int>>().ToTable("Fan_UserRole");
            builder.Entity<IdentityUserLogin<int>>().ToTable("Fan_UserLogin");
            builder.Entity<IdentityUserToken<int>>().ToTable("Fan_UserToken");

            builder.Entity<Meta>(entity =>
            {
                entity.ToTable("Fan_Meta");
                entity.HasKey(e => e.Id).ForSqlServerIsClustered(clustered: false);
                entity.HasIndex(e => e.Key).IsUnique().ForSqlServerIsClustered();
            });

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

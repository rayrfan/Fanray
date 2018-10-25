using Fan.Blog.Models;
using Fan.Data;
using Microsoft.EntityFrameworkCore;

namespace Fan.Blog.Data
{
    /// <summary>
    /// The blog app entity model.
    /// </summary>
    public class BlogEntityModelBuilder : IEntityModelBuilder
    {
        public void CreateModel(ModelBuilder builder)
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

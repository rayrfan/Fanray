using Fan.Blogs.Enums;
using Fan.Blogs.Models;
using Fan.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fan.Blogs.Data
{
    /// <summary>
    /// Sql implementation of the <see cref="ITagRepository"/> contract.
    /// </summary>
    public class SqlTagRepository : EFRepository<Tag>, ITagRepository
    {
        private readonly BlogDbContext _db;
        public SqlTagRepository(BlogDbContext db) : base(db)
        {
            _db = db;
        }

        /// <summary>
        /// Deletes a tag and its associated posts.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <remarks>
        /// Tag and PostTag are related by FK with cascade delete.
        /// </remarks>
        public async Task DeleteAsync(int id)
        {
            var tag = await _db.Tags.SingleAsync(t => t.Id == id);
            _db.Remove(tag); // cascade delete will take care of PostTag assoc
            await _db.SaveChangesAsync();
        }

        public async Task<List<Tag>> GetListAsync()
        {
            return await (from t in _db.Tags
                          select new Tag
                          {
                              Id = t.Id,
                              Title = t.Title,
                              Slug = t.Slug,
                              Color = t.Color,
                              Description = t.Description,
                              Count = (from p in _db.Posts
                                       from pt in p.PostTags
                                       where pt.TagId == t.Id && p.Status == EPostStatus.Published
                                       select pt).Count(),
                          }).ToListAsync();
        }
    }
}

using Fan.Enums;
using Fan.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fan.Data
{
    /// <summary>
    /// Sql implementation of the <see cref="ITagRepository"/> contract.
    /// </summary>
    public class SqlTagRepository : ITagRepository
    {
        private readonly FanDbContext _db;
        public SqlTagRepository(FanDbContext db)
        {
            _db = db;
        }

        public async Task<Tag> CreateAsync(Tag tag)
        {
            await _db.Tags.AddAsync(tag);
            await _db.SaveChangesAsync();
            return tag;
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

        public async Task<Tag> UpdateAsync(Tag tag)
        {
            await _db.SaveChangesAsync();
            return tag;
        }
    }
}

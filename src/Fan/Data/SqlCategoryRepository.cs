using Fan.Enums;
using Fan.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fan.Data
{
    /// <summary>
    /// Sql implementation of the <see cref="ICategoryRepository"/> contract.
    /// </summary>
    public class SqlCategoryRepository : ICategoryRepository
    {
        private readonly FanDbContext _db;
        public SqlCategoryRepository(FanDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Creates a new <see cref="Category"/>, the returned object is tracked.
        /// </summary>
        public async Task<Category> CreateAsync(Category category)
        {
            await _db.AddAsync(category);
            await _db.SaveChangesAsync();
            return category;
        }

        /// <summary>
        /// Deletes a <see cref="Category"/> by id and re-categorize its posts to the given 
        /// default category id.
        /// </summary>
        /// <remarks>
        /// Since <see cref="Post.CategoryId"/> is nullable, there is no Cascade Delete between 
        /// Post and Category, which happens to be what we want.  User can choose to delete a
        /// category and we should delete all posts associated with that category, instead we
        /// apply the default category on these posts. 
        /// 
        /// The defaultCategoryId is BlogSettings DefaultCategoryId, I choose to have it pass in
        /// from BLL for convenience instead of querying Meta for it.
        /// </remarks>
        public async Task DeleteAsync(int id, int defaultCategoryId)
        {
            if (id == defaultCategoryId) return;

            // remove it
            var category = await _db.Categories.SingleAsync(c => c.Id == id);
            _db.Remove(category);

            // update its posts to default category
            var posts = _db.Posts.Where(p => p.CategoryId == id);
            foreach (var post in posts)
            {
                post.CategoryId = defaultCategoryId;
            }

            await _db.SaveChangesAsync();
        }

        /// <summary>
        /// Returns a list of <see cref="Category"/>, the returned objects are not tracked.
        /// </summary>
        public async Task<List<Category>> GetListAsync()
        {
            return await _db.Categories.Select(
                    c => new Category
                    {
                        Id = c.Id,
                        Title = c.Title,
                        Slug = c.Slug,
                        Count = _db.Posts.Where(p => p.CategoryId == c.Id && p.Status == EPostStatus.Published).Count(),
                    }).ToListAsync();
        }

        /// <summary>
        /// Updates a <see cref="Category"/>.
        /// </summary>
        /// <param name="category">this parm is not used just being returned.</param>
        public async Task<Category> UpdateAsync(Category category)
        {
            await _db.SaveChangesAsync();
            return category;
        }
    }
}

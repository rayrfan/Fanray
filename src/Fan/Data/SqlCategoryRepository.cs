using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Fan.Models;

namespace Fan.Data
{
    public class SqlCategoryRepository : ICategoryRepository
    {
        private readonly FanDbContext _db;
        public SqlCategoryRepository(FanDbContext db)
        {
            _db = db;
        }

        public async Task<Category> CreateAsync(Category category)
        {
            await _db.AddAsync(category);
            await _db.SaveChangesAsync();
            return category;
        }

        public Task DeleteAsync(int id, int defaultCategoryId)
        {
            throw new NotImplementedException();
        }

        public Task<List<Category>> GetListAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Category> UpdateAsync(Category category)
        {
            throw new NotImplementedException();
        }
    }
}

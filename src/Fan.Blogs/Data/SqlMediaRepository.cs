using Fan.Blogs.Models;
using Fan.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Fan.Blogs.Data
{
    public class SqlMediaRepository : EntityRepository<Media>, IMediaRepository
    {
        private readonly FanDbContext _db;
        public SqlMediaRepository(FanDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<Media> GetAsync(string fileName, DateTimeOffset uploadedOn)
        {
            return await _entities.SingleOrDefaultAsync(m =>
                        m.FileName.Equals(fileName, StringComparison.InvariantCultureIgnoreCase) &&
                        m.UploadedOn.Year == uploadedOn.Year &&
                        m.UploadedOn.Month == uploadedOn.Month);
        }

        public async Task<Media> GetAsync(int mediaId)
        {
            return await _entities.SingleAsync(m => m.Id == mediaId);
        }
    }
}

using Fan.Blogs.Models;
using Fan.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Fan.Blogs.Data
{
    public class SqlMediaRepository : EFRepository<Media>, IMediaRepository
    {
        private readonly BlogDbContext _db;
        public SqlMediaRepository(BlogDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<Media> GetAsync(string fileName, DateTimeOffset uploadedOn)
        {
            return await _db.Medias.SingleOrDefaultAsync(m =>
                        m.FileName.Equals(fileName, StringComparison.InvariantCultureIgnoreCase) &&
                        m.UploadedOn.Year == uploadedOn.Year &&
                        m.UploadedOn.Month == uploadedOn.Month);
        }

        public async Task<Media> GetAsync(int mediaId)
        {
            return await _db.Medias.SingleAsync(m => m.Id == mediaId);
        }
    }
}

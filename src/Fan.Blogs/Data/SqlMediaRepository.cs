using Fan.Blogs.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Fan.Blogs.Data
{
    public class SqlMediaRepository : IMediaRepository
    {
        private readonly BlogDbContext _db;
        public SqlMediaRepository(BlogDbContext db)
        {
            _db = db;
        }

        public async Task<Media> CreateAsync(Media media)
        {
            await _db.Medias.AddAsync(media);
            await _db.SaveChangesAsync();
            return media;
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

        public async Task<Media> UpdateAsync(Media media)
        {
            await _db.SaveChangesAsync();
            return media;
        }
    }
}

using Fan.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fan.Medias
{
    public class SqlMediaRepository : EntityRepository<Media>, IMediaRepository
    {
        private readonly FanDbContext _db;
        public SqlMediaRepository(FanDbContext db) : base(db)
        {
            _db = db;
        }

        /// <summary>
        /// Returns <see cref="Media"/> by filename and upload date, returns null if not found.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="uploadedOn"></param>
        /// <returns></returns>
        public async Task<Media> GetAsync(string fileName, DateTimeOffset uploadedOn)
        {
            return await _entities.SingleOrDefaultAsync(m =>
                        m.FileName == fileName &&
                        m.UploadedOn.Year == uploadedOn.Year &&
                        m.UploadedOn.Month == uploadedOn.Month);
        }

        public async Task<(List<Media> medias, int count)> GetMediasAsync(EMediaType mediaType, int pageNumber, int pageSize)
        {
            int skip = (pageNumber - 1) * pageSize;
            int take = pageSize;

            var q = _entities.Where(m => m.MediaType == mediaType);
            var medias = await q.OrderByDescending(m => m.UploadedOn)
                                .Skip(skip)
                                .Take(take)
                                .ToListAsync();
            var count = await q.CountAsync();

            return (medias: medias, count: count);
        }
    }
}

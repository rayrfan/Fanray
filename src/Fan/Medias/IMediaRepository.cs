using Fan.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fan.Medias
{
    public interface IMediaRepository : IRepository<Media>
    {
        /// <summary>
        /// Returns <see cref="Media"/> by id.
        /// </summary>
        /// <param name="mediaId"></param>
        /// <returns></returns>
        Task<Media> GetAsync(int mediaId);
        /// <summary>
        /// Returns <see cref="Media"/> by filename and upload date.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="uploadedOn"></param>
        /// <returns></returns>
        Task<Media> GetAsync(string fileName, DateTimeOffset uploadedOn);
        /// <summary>
        /// Returns a list of <see cref="Media"/> records.
        /// </summary>
        /// <param name="mediaType"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        Task<List<Media>> GetMediasAsync(EMediaType mediaType, int pageNumber, int pageSize);
    }
}

using Fan.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fan.Medias
{
    /// <summary>
    /// Contract for a media repository.
    /// </summary>
    /// <remarks>
    /// This class should not be used outside of this DLL except when register for DI in Startup.cs, 
    /// all media operations should be called through <see cref="IMediaService"/>.
    /// </remarks>
    public interface IMediaRepository : IRepository<Media>
    {
        /// <summary>
        /// Deletes a media by id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task DeleteAsync(int id);

        /// <summary>
        /// Returns <see cref="Media"/> by filename and upload date.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="uploadedOn"></param>
        /// <returns></returns>
        Task<Media> GetAsync(string fileName, DateTimeOffset uploadedOn);

        /// <summary>
        /// Returns a list of <see cref="Media"/> based on media type page number and page size, 
        /// or empty list if no records found; and total count of medias for this media type.
        /// </summary>
        /// <param name="mediaType"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        Task<(List<Media> medias, int count)> GetMediasAsync(EMediaType mediaType, int pageNumber, int pageSize);
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Fan.Medias
{
    /// <summary>
    /// The media service manages uploaded media files.
    /// </summary>
    /// <remarks>
    /// For images it resizes them, stores them away with storage provider, gets a unique filename,
    /// encode a title from filename, generates an image url.
    /// </remarks>
    public interface IMediaService
    {
        /// <summary>
        /// Deletes a media by id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task DeleteMediaAsync(int id);

        /// <summary>
        /// Returns true if media with the search criteria exists, false otherwise.
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        Task<bool> ExistsAsync(Expression<Func<Media, bool>> predicate);

        /// <summary>
        /// Returns list of media with the search criteria.
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        Task<IEnumerable<Media>> FindAsync(Expression<Func<Media, bool>> predicate);

        /// <summary>
        /// Returns the media by id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<Media> GetMediaAsync(int id);

        /// <summary>
        /// Returns a list of <see cref="Media"/> records based on search critria.
        /// </summary>
        /// <param name="mediaType"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        Task<List<Media>> GetMediasAsync(EMediaType mediaType, int pageNumber, int pageSize);

        /// <summary>
        /// Updates media title and description.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="title"></param>
        /// <param name="caption"></param>
        /// <param name="alt"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        Task<Media> UpdateMediaAsync(int id, 
            string title, 
            string caption,
            string alt,
            string description);

        /// <summary>
        /// Uploads an image stream by resizing and storing multiple copies of it.
        /// </summary>
        /// <param name="source">The image stream.</param>
        /// <param name="resizes">Image resizes.</param>
        /// <param name="fileName">The processed unique filename.</param>
        /// <param name="contentType">The file content type.</param>
        /// <param name="uploadedOn">The datetimeoffset the image is uploaed.</param>
        /// <param name="appType">The app that is uploading it.</param>
        /// <param name="userId">The user who uploads the image.</param>
        /// <param name="uploadFrom">The agent that uploads the image.</param>
        /// <returns></returns>
        Task<Media> UploadImageAsync(Stream source,
            List<ImageResizeInfo> resizes,
            string fileName,
            string contentType,
            string title,
            DateTimeOffset uploadedOn,
            EAppType appType,
            int userId,
            EUploadedFrom uploadFrom = EUploadedFrom.Browser);
    }
}

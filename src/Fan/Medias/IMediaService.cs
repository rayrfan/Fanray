using System;
using System.Collections.Generic;
using System.IO;
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
        Task<Media> UpdateMediaAsync(int id, string title, string description);

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

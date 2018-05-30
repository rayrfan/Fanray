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
        /// Returns image handler url after uploading image byte array to storage. 
        /// </summary>
        /// <remarks>
        /// This is currently only used by olw. Image is not resized.
        /// </remarks>
        /// <param name="source">File content</param>
        /// <param name="appType">Which fanray app it uploaded it.</param>
        /// <param name="userId">Id of the user uploading the media.</param>
        /// <param name="fileNameOrig">File name with ext.</param>
        /// <param name="uploadFrom">Which client uploaded it.</param>
        /// <returns></returns>
        Task<string> UploadImageAsync(byte[] source, EAppType appType, int userId, string fileNameOrig, EUploadedFrom uploadFrom);
        /// <summary>
        /// Returns image url after uploading image stream to storage.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="appType"></param>
        /// <param name="userId"></param>
        /// <param name="fileNameOrig"></param>
        /// <param name="uploadFrom"></param>
        /// <returns></returns>
        Task<string> UploadImageAsync(Stream source, EAppType appType, int userId, string fileNameOrig, EUploadedFrom uploadFrom);
        /// <summary>
        /// Updates media title and description.
        /// </summary>
        Task<Media> UpdateMediaAsync(int id, string title, string description);
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

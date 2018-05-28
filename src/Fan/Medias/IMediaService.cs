using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Fan.Medias
{
    public interface IMediaService
    {
        /// <summary>
        /// Returns image url after uploading image byte array to storage.
        /// </summary>
        /// <remarks>
        /// This is currently only used by olw. Image is not resized.
        /// </remarks>
        /// <param name="userId">Id of the user uploading the media.</param>
        /// <param name="fileName">File name with ext.</param>
        /// <param name="source">File content</param>
        /// <param name="appId">Which fanray app it uploaded it.</param>
        /// <param name="uploadFrom">Which client uploaded it.</param>
        /// <returns></returns>
        Task<string> UploadImageAsync(byte[] source, EAppType appId, int userId, string name, EUploadedFrom uploadFrom);
        /// <summary>
        /// Returns image url after uploading and resizeing image stream to storage.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="appId"></param>
        /// <param name="userId"></param>
        /// <param name="fileName"></param>
        /// <param name="uploadFrom"></param>
        /// <returns></returns>
        Task<string> UploadImageAsync(Stream source, EAppType appId, int userId, string fileName, EUploadedFrom uploadFrom);
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
        Task<List<Media>> GetMediasAsync(EMediaType mediaType, int pageNumber = 1, int pageSize = 50);
    }
}

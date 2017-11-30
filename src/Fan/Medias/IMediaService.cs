using System.Threading.Tasks;

namespace Fan.Medias
{
    public interface IMediaService
    {
        /// <summary>
        /// Returns media url after upload to storage.
        /// </summary>
        /// <param name="userId">Id of the user uploading the media.</param>
        /// <param name="fileName">File name with ext.</param>
        /// <param name="content">File content</param>
        /// <param name="appId">Which fanray app it uploaded it.</param>
        /// <param name="uploadFrom">Which client uploaded it.</param>
        /// <returns></returns>
        Task<string> UploadMediaAsync(int userId, string name, byte[] content, EAppType appId, EUploadedFrom uploadFrom);
        /// <summary>
        /// Updates media title and description.
        /// </summary>
        Task<Media> UpdateMediaAsync(int id, string title, string description);
    }
}

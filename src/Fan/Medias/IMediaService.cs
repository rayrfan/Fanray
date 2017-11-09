using System.Threading.Tasks;

namespace Fan.Medias
{
    public interface IMediaService
    {
        /// <summary>
        /// Uploads media and inserts a record in datasource.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="name"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        Task<string> UploadMediaAsync(string userName, string name, byte[] content, EAppType appId);
        /// <summary>
        /// Updates media title and description.
        /// </summary>
        Task<Media> UpdateMediaAsync(int id, string title, string description);
    }
}

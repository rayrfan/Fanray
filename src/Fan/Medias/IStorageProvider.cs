using System.IO;
using System.Threading.Tasks;

namespace Fan.Medias
{
    /// <summary>
    /// The storage provider save the incoming file whether its byte[] or stream into the storage.
    /// It makes sure it gets a unique filename by looking at what is already in storage.
    /// </summary>
    public interface IStorageProvider
    {
        /// <summary>
        /// The absolute URI endpoint to resource.
        /// </summary>
        string StorageEndpoint { get; }

        /// <summary>
        /// Saves file to storage.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="info"></param>
        /// <param name="fileNameUnique"></param>
        /// <returns></returns>
        Task SaveFileAsync(Stream source, ImageResizeInfo info, string fileNameUnique);     
    }
}

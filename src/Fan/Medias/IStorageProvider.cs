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
        Task SaveFileAsync(Stream source, string fileName, string path, char pathSeparator);

        /// <summary>
        /// Deletes a file from storage.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="path"></param>
        /// <param name="pathSeparator"></param>
        /// <returns></returns>
        Task DeleteFileAsync(string fileName, string path, char pathSeparator);
    }
}

using System;
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
        /// Returns unqiue file name after saveing file byte array to storage.
        /// </summary>
        /// <remarks>
        /// The storage type can be configured in appsettings.json. The file is stored like the following
        /// "container/appName/userId/year/month/fileName.ext".
        /// </remarks>
        /// <param name="source">The bytes of the file.</param>
        /// <param name="appId">Which app uploaded file.</param>
        /// <param name="userId">Who uploaded the file.</param>
        /// <param name="uploadedOn">When uploaded.</param>
        /// <param name="fileName">Slugged filename with ext.</param>
        /// <param name="size">Optimized or original</param>
        Task<string> SaveFileAsync(byte[] source, EAppType appId, int userId, DateTimeOffset uploadedOn, string fileName, EImageSize size);

        /// <summary>
        /// Returns unqiue file name after saveing file stream to storage.
        /// </summary>
        /// <remarks>
        /// The storage type can be configured in appsettings.json. The file is stored like the following
        /// "container/appName/userId/year/month/fileName.ext".
        /// </remarks>
        /// <param name="source">The stream of the file.</param>
        /// <param name="appId">Which app uploaded file.</param>
        /// <param name="userId">Who uploaded the file.</param>
        /// <param name="uploadedOn">Upload year.</param>
        /// <param name="fileName">Slugged filename with ext.</param>
        /// <param name="size">Optimized or original</param>
        Task<string> SaveFileAsync(Stream source, EAppType appId, int userId, DateTimeOffset uploadedOn, string fileName, EImageSize size);
    }
}

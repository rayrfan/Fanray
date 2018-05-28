using System.IO;
using System.Threading.Tasks;

namespace Fan.Medias
{
    /// <summary>
    /// Storage provider contract.
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
        /// <param name="year">Upload year.</param>
        /// <param name="month">Upload month.</param>
        /// <param name="fileName">Slugged filename with ext.</param>
        Task<string> SaveFileAsync(byte[] source, EAppType appId, int userId, string year, string month, string fileName);

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
        /// <param name="year">Upload year.</param>
        /// <param name="month">Upload month.</param>
        /// <param name="fileName">Slugged filename with ext.</param>
        Task<string> SaveFileAsync(Stream source, EAppType appId, int userId, string year, string month, string fileName);
    }
}

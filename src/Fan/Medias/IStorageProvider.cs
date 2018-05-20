using System.Threading.Tasks;

namespace Fan.Medias
{
    /// <summary>
    /// Storage provider contract.
    /// </summary>
    public interface IStorageProvider
    {
        /// <summary>
        /// Saves file to storage and returns unqiue file name.
        /// </summary>
        /// <returns>
        /// The file name that is unique, there could be name conflict.
        /// </returns>
        /// <remarks>
        /// The storage type can be configured in appsettings.json. The file is stored like the following
        /// "container/appType/userId/year/month/fileName.ext".
        /// </remarks>
        /// <param name="userId">The id of the user who uploads.</param>
        /// <param name="fileName">Slugged filename with ext.</param>
        /// <param name="year">Upload year.</param>
        /// <param name="month">Upload month.</param>
        /// <param name="content">The content of file.</param>
        /// <param name="appId">Which app it uploaded it.</param>
        Task<string> SaveFileAsync(int userId, string fileName, string year, string month, byte[] content, EAppType appId);
    }
}

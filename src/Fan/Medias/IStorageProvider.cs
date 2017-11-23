using System.Threading.Tasks;

namespace Fan.Medias
{
    /// <summary>
    /// Storage provider contract.
    /// </summary>
    public interface IStorageProvider
    {
        /// <summary>
        /// Saves file to storage.
        /// </summary>
        /// <param name="fileName">Slugged filename with ext.</param>
        /// <param name="year">Upload year.</param>
        /// <param name="month">Upload month.</param>
        /// <param name="content">The content of file.</param>
        /// <param name="appId">Which app it uploaded it.</param>
        Task<string> SaveFileAsync(string fileName, string year, string month, byte[] content, EAppType appId);
    }
}

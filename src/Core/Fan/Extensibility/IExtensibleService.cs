using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fan.Extensibility
{
    /// <summary>
    /// The extensible service interface.
    /// </summary>
    /// <typeparam name="TInfo"></typeparam>
    /// <typeparam name="TExtension"></typeparam>
    public interface IExtensibleService<TInfo, TExtension> 
        where TInfo : class
        where TExtension  : class
    {
        /// <summary>
        /// Returns a list of manifest info.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<TInfo>> GetInstalledManifestInfosAsync();

        /// <summary>
        /// Returns an extension of the real derived type.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<TExtension> GetExtensionAsync(int id);

        /// <summary>
        /// Installs an extension by getting the entension's files on server.
        /// </summary>
        /// <returns></returns>
        Task InstallExtensionAsync(TExtension extension);

        /// <summary>
        /// Returns true if the extension's folder name is valid, false otherwise.
        /// </summary>
        /// <param name="folder">The folder name.</param>
        /// <returns></returns>
        bool IsValidExtensionFolder(string folder);
    }
}

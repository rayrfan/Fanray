using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fan.Extensibility
{
    public interface IExtensibleService<T> where T : ManifestInfo
    {
        /// <summary>
        /// Returns a list of manifest info type T.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<T>> GetInstalledManifestInfosAsync();
    }
}

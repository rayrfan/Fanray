using Fan.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fan.Extensibility
{
    /// <summary>
    /// The extensible service.
    /// </summary>
    /// <typeparam name="TManifest"></typeparam>
    /// <typeparam name="TExtension"></typeparam>
    public abstract class ExtensibleService<TManifest, TExtension> : IExtensibleService<TManifest, TExtension> 
        where TManifest : Manifest
        where TExtension : Extension
    {
        protected readonly IMetaRepository metaRepository;
        protected readonly IDistributedCache distributedCache;
        protected readonly IHostingEnvironment hostingEnvironment;

        public ExtensibleService(IMetaRepository metaRepository,
            IDistributedCache distributedCache,
            IHostingEnvironment hostingEnvironment)
        {
            this.metaRepository = metaRepository;
            this.distributedCache = distributedCache;
            this.hostingEnvironment = hostingEnvironment;
        }

        /// <summary>
        /// Returns a list of manifests.
        /// </summary>
        /// <returns></returns>
        public abstract Task<IEnumerable<TManifest>> GetInstalledManifestsAsync();

        /// <summary>
        /// Returns an extension of the real derived type.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<TExtension> GetExtensionAsync(int id)
        {
            var meta = await metaRepository.GetAsync(id);
            var baseType = JsonConvert.DeserializeObject<TExtension>(meta.Value);
            var actualType = await GetManifestTypeByFolderAsync(baseType.Folder);
            var extension = (TExtension)JsonConvert.DeserializeObject(meta.Value, actualType);

            return extension;
        }

        /// <summary>
        /// Installs an extension.
        /// </summary>
        /// <param name="extension"></param>
        /// <returns></returns>
        public Task InstallExtensionAsync(TExtension extension)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns true if the extension's folder name is valid, false otherwise.
        /// </summary>
        /// <param name="folder">The folder name.</param>
        /// <returns></returns>
        public abstract bool IsValidExtensionFolder(string folder);

        /// <summary>
        /// Returns extension manifest by folder.
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        protected async Task<TManifest> GetManifestByFolderAsync(string folder)
        {
            var manifests = await GetInstalledManifestsAsync();
            return manifests.Single(wi => wi.Folder.Equals(folder, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Returns extension type by folder.
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        internal protected async Task<Type> GetManifestTypeByFolderAsync(string folder)
        {
            var manifest = await GetManifestByFolderAsync(folder);
            return Type.GetType(manifest.Type);
        }
    }
}

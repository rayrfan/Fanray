using Fan.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
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
        protected readonly ILogger<IExtensibleService<TManifest, TExtension>> logger;

        public ExtensibleService(IMetaRepository metaRepository,
            IDistributedCache distributedCache,
            IHostingEnvironment hostingEnvironment, 
            ILogger<IExtensibleService<TManifest, TExtension>> logger)
        {
            this.metaRepository = metaRepository;
            this.distributedCache = distributedCache;
            this.hostingEnvironment = hostingEnvironment;
            this.logger = logger;
        }

        /// <summary>
        /// The manifest file name.
        /// </summary>
        public abstract string ManifestName { get; }

        /// <summary>
        /// The manifest file containing directory.
        /// </summary>
        public abstract string ManifestDirectory { get; }

        /// <summary>
        /// Returns a list of manifests.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Each extension implementation may have specific logic to process manifest data.
        /// </remarks>
        public abstract Task<IEnumerable<TManifest>> GetManifestsAsync();

        /// <summary>
        /// Returns an extension of the real derived type.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual async Task<TExtension> GetExtensionAsync(int id)
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
            var manifests = await LoadManifestsAsync();
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

        /// <summary>
        /// Returns installed extensions' manifests.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// This method serves as a default implementation to support GetManifestsAsync method.
        /// It scans a particular extension folder and reads all the manifest json files for each extension.
        /// </remarks>
        protected async Task<IEnumerable<TManifest>> LoadManifestsAsync()
        {
            var list = new List<TManifest>();
            var extPath = Path.Combine(hostingEnvironment.ContentRootPath, ManifestDirectory);

            foreach (var dir in Directory.GetDirectories(extPath))
            {
                var file = Path.Combine(dir, ManifestName);
                var manifest = JsonConvert.DeserializeObject<TManifest>(await File.ReadAllTextAsync(file));
                manifest.Folder = new DirectoryInfo(dir).Name;
                if (!IsValidExtensionFolder(manifest.Folder)) continue;

                if (manifest.Type.IsNullOrEmpty())
                {
                    logger.LogError($"Invalid {ManifestName} in {manifest.Folder}, missing \"type\" information.");
                }
                else
                {
                    list.Add(manifest);
                }
            }

            return list;
        }
    }
}

using Fan.Data;
using Fan.Exceptions;
using Fan.Extensibility;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Fan.Plugins
{
    /// <summary>
    /// The plugin service.
    /// </summary>
    public class PluginService : ExtensibleService<PluginManifest, Plugin>, IPluginService
    {
        /// <summary>
        /// The manifest file name for plugins "plugin.json".
        /// </summary>
        public const string PLUGIN_MANIFEST = "plugin.json";
        /// <summary>
        /// The directory that contains plugins "Plugins".
        /// </summary>
        public const string PLUGIN_DIR = "Plugins";
        /// <summary>
        /// A plugin's folder must be in PascalCase.
        /// </summary>
        public const string PLUGIN_FOLDER_REGEX = @"^[A-Z][a-z]+(?:[A-Z][a-z]+)*$";

        private const string CACHE_KEY_INSTALLED_PLUGIN_MANIFESTS = "installed-plugin-manifests";
        private TimeSpan Cache_Time_Installed_Plugin_Manifests = new TimeSpan(0, 10, 0);
        private const string CACHE_KEY_ACTIVE_PLUGINS = "active-plugins";
        private TimeSpan Cache_Time_Active_Plugins = new TimeSpan(0, 10, 0);

        private readonly ILogger<PluginService> logger;

        public PluginService(IHostingEnvironment hostingEnvironment,
                        IDistributedCache distributedCache,
                        IMetaRepository metaRepository,
                        ILogger<PluginService> logger) 
            : base(metaRepository, distributedCache, hostingEnvironment)
        {
            this.logger = logger;
        }

        // -------------------------------------------------------------------- public methods

        /// <summary>
        /// Activates a plugin.
        /// </summary>
        /// <param name="folder">The key of the plugin.</param>
        /// <returns>Id of the plugin.</returns>
        /// <remarks>
        /// It upserts a plugin meta and makes sure Active is true.
        /// </remarks>
        public async Task<int> ActivatePluginAsync(string folder)
        {
            var meta = await GetPluginMetaAsync(folder);
            if (meta != null)
            {
                var plugin = JsonConvert.DeserializeObject<Plugin>(meta.Value);
                plugin.Active = true;
                await UpdatePluginAsync(plugin);
            }
            else
            {
                // insert plugin meta
                var manifest = await GetManifestByFolderAsync(folder);
                var type = Type.GetType(manifest.Type);
                var plugin = (Plugin)Activator.CreateInstance(type);
                plugin.Folder = folder;
                plugin.Active = true;

                meta = await metaRepository.CreateAsync(new Meta
                {
                    Key = folder.ToLower(),  // plugin key is lower
                    Value = JsonConvert.SerializeObject(plugin),
                    Type = EMetaType.Plugin,
                });
            }

            await distributedCache.RemoveAsync(CACHE_KEY_ACTIVE_PLUGINS);
            await distributedCache.RemoveAsync(CACHE_KEY_INSTALLED_PLUGIN_MANIFESTS);

            return meta.Id;
        }

        /// <summary>
        /// De-activates a plugin.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <remarks>
        /// De-activation removes plugin id from active-plugins but does not delete the plugin meta.
        /// </remarks>
        public async Task DeactivatePluginAsync(int id)
        {
            var plugin = await GetExtensionAsync(id);
            plugin.Active = false;
            await UpdatePluginAsync(plugin);

            await distributedCache.RemoveAsync(CACHE_KEY_ACTIVE_PLUGINS);
            await distributedCache.RemoveAsync(CACHE_KEY_INSTALLED_PLUGIN_MANIFESTS);
        }

        /// <summary>
        /// Returns a list of active plugins.
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<Plugin>> GetActivePluginsAsync()
        {
            return await distributedCache.GetAsync(CACHE_KEY_ACTIVE_PLUGINS, Cache_Time_Active_Plugins, async () =>
            {
                var activePlugins = new List<Plugin>();
                var pluginMetas = await metaRepository.FindAsync(m => m.Type == EMetaType.Plugin);

                foreach (var meta in pluginMetas)
                {
                    var plugin = await GetExtensionAsync(meta.Id);
                    if (plugin.Active)
                    {
                        plugin.Id = meta.Id; // TODO
                        activePlugins.Add(plugin);
                    }
                }

                return activePlugins;
            }, includeTypeName: true);
        }

        /// <summary>
        /// Returns a list of plugin manifests.
        /// </summary>
        /// <returns></returns>
        public override async Task<IEnumerable<PluginManifest>> GetInstalledManifestsAsync()
        {
            return await distributedCache.GetAsync(CACHE_KEY_INSTALLED_PLUGIN_MANIFESTS, Cache_Time_Installed_Plugin_Manifests, async () =>
            {
                var list = new List<PluginManifest>();
                var pluginsFolder = Path.Combine(hostingEnvironment.ContentRootPath, PLUGIN_DIR);

                foreach (var dir in Directory.GetDirectories(pluginsFolder))
                {
                    var file = Path.Combine(dir, PLUGIN_MANIFEST);
                    var manifest = JsonConvert.DeserializeObject<PluginManifest>(await File.ReadAllTextAsync(file));
                    manifest.Folder = new DirectoryInfo(dir).Name;
                    if (!IsValidExtensionFolder(manifest.Folder)) continue;

                    if (manifest.Type.IsNullOrEmpty())
                    {
                        logger.LogError($"Invalid {PLUGIN_MANIFEST} in {manifest.Folder}, missing \"type\" information.");
                    }
                    else
                    {
                        var meta = await GetPluginMetaAsync(manifest.Folder);
                        if (meta != null)
                        {
                            var plugin = JsonConvert.DeserializeObject<Plugin>(meta.Value);
                            manifest.Active = plugin.Active;
                            manifest.Id = meta.Id;
                        }
                        list.Add(manifest);
                    }
                }

                return list;
            });
        }

        /// <summary>
        /// Returns true if plugin folder name is valid.
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        public override bool IsValidExtensionFolder(string folder) => new Regex(PLUGIN_FOLDER_REGEX).IsMatch(folder);

        /// <summary>
        /// Updates a plugin settings.
        /// </summary>
        /// <param name="plugin">The plugin data/settings.</param>
        /// <remarks>
        /// This method takes pre-caution if plugin meta does not exist for some reason
        /// it creates it.
        /// </remarks>
        public async Task UpdatePluginAsync(Plugin plugin)
        {
            var meta = await GetPluginMetaAsync(plugin.Folder);
            if (meta == null)
                throw new FanException($"Plugin {plugin.Folder} is not found.");

            meta.Value = JsonConvert.SerializeObject(plugin);
            await metaRepository.UpdateAsync(meta);

            await distributedCache.RemoveAsync(CACHE_KEY_ACTIVE_PLUGINS);
        }

        // -------------------------------------------------------------------- private helpers

        /// <summary>
        /// Returns plugin meta record.
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        private async Task<Meta> GetPluginMetaAsync(string folder)
        {
            var key = folder.ToLower(); // plugin key is lower
            return await metaRepository.GetAsync(key, EMetaType.Plugin);
        }
    }
}

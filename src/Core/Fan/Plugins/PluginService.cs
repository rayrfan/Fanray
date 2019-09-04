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
using System.Linq;
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
        /// The plugin directory, "Plugins".
        /// </summary>
        public const string PLUGIN_DIR = "Plugins";
        /// <summary>
        /// The system plugin directory, "SysPlugins".
        /// </summary>
        public const string SYS_PLUGIN_DIR = "SysPlugins";

        private const string CACHE_KEY_PLUGIN_MANIFESTS = "plugin-manifests";
        private TimeSpan Cache_Time_Plugin_Manifests = new TimeSpan(0, 10, 0);
        private const string CACHE_KEY_ACTIVE_PLUGINS = "active-plugins";
        private TimeSpan Cache_Time_Active_Plugins = new TimeSpan(0, 10, 0);

        public PluginService(IHostingEnvironment hostingEnvironment,
                        IDistributedCache distributedCache,
                        IMetaRepository metaRepository,
                        ILogger<PluginService> logger) 
            : base(metaRepository, distributedCache, hostingEnvironment, logger)
        {
        }

        public override string ManifestName { get; } = PLUGIN_MANIFEST;
        public override string ManifestDirectory { get; } = PLUGIN_DIR;

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
            if (folder.IsNullOrEmpty()) throw new ArgumentNullException("Cannot activate a plugin with an empty name.");

            var manifests = await LoadManifestsAsync();
            var manifest = manifests.SingleOrDefault(m => m.Folder.ToUpperInvariant().Equals(folder.ToUpperInvariant()));
            if (manifest.IsSysPlugin)
                throw new FanException("A system plugin cannot be activated.");

            var meta = await GetPluginMetaAsync(folder);
            if (meta != null)
            {
                var plugin = await GetExtensionAsync(meta.Id);
                if (!plugin.Active)
                {
                    plugin.Active = true;
                    meta.Value = JsonConvert.SerializeObject(plugin);
                    await metaRepository.UpdateAsync(meta);
                }
            }
            else
            {
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
            await distributedCache.RemoveAsync(CACHE_KEY_PLUGIN_MANIFESTS);

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
            var meta = await GetPluginMetaAsync(plugin.Folder);
            plugin.Active = false;
            meta.Value = JsonConvert.SerializeObject(plugin);
            await metaRepository.UpdateAsync(meta);

            await distributedCache.RemoveAsync(CACHE_KEY_ACTIVE_PLUGINS);
            await distributedCache.RemoveAsync(CACHE_KEY_PLUGIN_MANIFESTS);
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
        /// Returns a list of manifests both plugins and system plugins. 
        /// </summary>
        /// <returns></returns>
        public override async Task<IEnumerable<PluginManifest>> GetManifestsAsync()
        {
            return await distributedCache.GetAsync(CACHE_KEY_PLUGIN_MANIFESTS, Cache_Time_Plugin_Manifests, async () =>
            {
                var list = new List<PluginManifest>();

                var manifests = await LoadManifestsAsync();
                foreach (var manifest in manifests)
                {
                    var meta = await GetPluginMetaAsync(manifest.Folder);
                    if (meta != null)
                    {
                        var plugin = await GetExtensionAsync(meta.Id);
                        manifest.Id = meta.Id;
                        manifest.Active = plugin.Active;
                        manifest.SettingsUrl = plugin.SettingsUrl;
                    }
                    else
                    {
                        var type = Type.GetType(manifest.Type);
                        var plugin = (Plugin)Activator.CreateInstance(type);
                        manifest.SettingsUrl = plugin.SettingsUrl;
                    }

                    list.Add(manifest);
                }

                return list;
            });            
        }

        /// <summary>
        /// Returns a plugin by id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public override async Task<Plugin> GetExtensionAsync(int id)
        {
            var plugin = await base.GetExtensionAsync(id);
            plugin.Id = id;
            return plugin;
        }

        /// <summary>
        /// Returns plugin by name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public async Task<Plugin> GetPluginAsync(string name)
        {
            Plugin plugin = null;
            var meta = await GetPluginMetaAsync(name);

            if (meta == null)
            {
                var manifests = await LoadManifestsAsync();
                var manifest = manifests.SingleOrDefault(m => m.Folder.ToUpperInvariant().Equals(name.ToUpperInvariant()));
                var actualType = Type.GetType(manifest.Type);
                plugin = (Plugin)Activator.CreateInstance(actualType);
                plugin.Active = manifest.Active;
                plugin.Folder = manifest.Folder;
            }
            else
            {
                var baseType = JsonConvert.DeserializeObject<Plugin>(meta.Value);
                var actualType = await GetManifestTypeByFolderAsync(baseType.Folder);
                plugin = (Plugin)JsonConvert.DeserializeObject(meta.Value, actualType);
            }

            return plugin;
        }

        /// <summary>
        /// Upserts a plugin settings.
        /// </summary>
        /// <param name="plugin"></param>
        public async Task<int> UpsertPluginAsync(Plugin plugin)
        {
            var meta = await GetPluginMetaAsync(plugin.Folder);
            if (meta == null)
            {
                meta = await metaRepository.CreateAsync(new Meta
                {
                    Key = plugin.Folder.ToLower(),  // plugin key is lower
                    Value = JsonConvert.SerializeObject(plugin),
                    Type = EMetaType.Plugin,
                });
            }
            else
            {
                meta.Value = JsonConvert.SerializeObject(plugin);
                await metaRepository.UpdateAsync(meta);
            }

            await distributedCache.RemoveAsync(CACHE_KEY_ACTIVE_PLUGINS);

            return meta.Id;
        }

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

        /// <summary>
        /// Returns all manifests of plugins and system plugins.
        /// </summary>
        /// <returns></returns>
        protected override async Task<IEnumerable<PluginManifest>> LoadManifestsAsync()
        {
            // plugin manifests
            var manifestsPlugins = await base.LoadManifestsAsync();

            // system plugin manifests
            var list = new List<PluginManifest>();
            var extPath = Path.Combine(hostingEnvironment.ContentRootPath, SYS_PLUGIN_DIR);
            foreach (var dir in Directory.GetDirectories(extPath))
            {
                var file = Path.Combine(dir, ManifestName);
                var manifest = JsonConvert.DeserializeObject<PluginManifest>(await File.ReadAllTextAsync(file));
                manifest.Folder = new DirectoryInfo(dir).Name;
                if (!IsValidExtensionFolder(manifest.Folder)) continue;

                if (manifest.Type.IsNullOrEmpty())
                {
                    logger.LogError($"Invalid System Plugin {ManifestName} in {manifest.Folder}, missing \"type\" information.");
                }
                else
                {
                    manifest.Active = true;
                    manifest.IsSysPlugin = true;
                    list.Add(manifest);
                }
            }

            return manifestsPlugins.Concat(list);
        }
    }
}

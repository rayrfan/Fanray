using Fan.Extensibility;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fan.Plugins
{
    public interface IPluginService : IExtensibleService<PluginInfo, Plugin>
    {
        /// <summary>
        /// Activates a plugin.
        /// </summary>
        /// <param name="folder"></param>
        /// <returns>Id of the plugin.</returns>
        Task<int> ActivatePluginAsync(string folder);

        /// <summary>
        /// De-activates a plugin.
        /// </summary>
        /// <returns></returns>
        Task DeactivatePluginAsync(int id);

        /// <summary>
        /// Deletes a plugin by deleting the plugin's files from server.
        /// </summary>
        /// <returns></returns>
        //Task DeletePluginAsync();

        /// <summary>
        /// Returns a list of active plugins.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<Plugin>> GetActivePluginsAsync();

        /// <summary>
        /// Updates a plugin settings.
        /// </summary>
        /// <param name="plugin"></param>
        /// <returns></returns>
        Task UpdatePluginAsync(Plugin plugin);
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Fan.Plugins
{
    public interface IPluginService
    {
        /// <summary>
        /// Returns all the installed plugins info.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<PluginInfo>> GetInstalledPluginsInfoAsync();
    }
}

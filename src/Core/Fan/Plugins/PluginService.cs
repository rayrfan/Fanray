using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Fan.Plugins
{
    public class PluginService : IPluginService
    {
        private readonly IHostingEnvironment hostingEnvironment;

        public PluginService(IHostingEnvironment hostingEnvironment)
        {
            this.hostingEnvironment = hostingEnvironment;
        }

        public async Task<IEnumerable<PluginInfo>> GetInstalledManifestInfosAsync()
        {
            var list = new List<PluginInfo>();
            var widgetsFolder = Path.Combine(hostingEnvironment.ContentRootPath, "Plugins");

            foreach (var dir in Directory.GetDirectories(widgetsFolder))
            {
                var file = Path.Combine(dir, "plugin.json");
                var info = JsonConvert.DeserializeObject<PluginInfo>(await File.ReadAllTextAsync(file));
                list.Add(info);
            }

            return list;
        }
    }
}

using Fan.Exceptions;
using Fan.Helpers;
using Fan.Medias;
using Fan.Plugins;
using Fan.Settings;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddPlugins(this IServiceCollection services, IHostingEnvironment hostingEnvironment)
        {
            var pluginsDir = Path.Combine(hostingEnvironment.ContentRootPath, "Plugins");
            var binDir = Util.IsRunningFromTestHost() ? Environment.CurrentDirectory :
                Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            foreach (var dir in Directory.GetDirectories(pluginsDir))
            {
                try
                {
                    // get plugin.json
                    var pluginJson = Path.Combine(dir, "plugin.json");
                    if (!File.Exists(pluginJson))
                    {
                        throw new FanException($"The plugin.json is not found.");
                    }

                    // load plugin dlls
                    var pluginInfo = JsonConvert.DeserializeObject<PluginInfo>(File.ReadAllText(pluginJson));
                    var dll = Path.Combine(binDir, pluginInfo.Dll);                   
                    var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(dll);

                    // configure plugins
                    var plugin = assembly.GetTypes().FirstOrDefault(t => typeof(Plugin).IsAssignableFrom(t));
                    if ((plugin != null) && (plugin != typeof(Plugin)))
                    {
                        services.AddSingleton(typeof(Plugin), plugin);
                    }
                }
                catch (Exception)
                {
                    continue;
                }
            }

            return services;
        }

        /// <summary>
        /// Adds a <see cref="IStorageProvider"/> based on your appsettings.json.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        /// <remarks>
        /// Another approach is to add all storage providers to the svc collection see https://bit.ly/2EHjEOS
        /// and let the classes that need storage to pick the right provider based on config.
        /// </remarks>
        public static IServiceCollection AddStorageProvider(this IServiceCollection services, IConfiguration configuration)
        {
            var appSettingsConfigSection = configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(appSettingsConfigSection);
            var appSettings = appSettingsConfigSection.Get<AppSettings>();
            if (appSettings.MediaStorageType == EMediaStorageType.AzureBlob)
                services.AddScoped<IStorageProvider, AzureBlobStorageProvider>();
            else
                services.AddScoped<IStorageProvider, FileSysStorageProvider>();

            return services;
        }
    }
}

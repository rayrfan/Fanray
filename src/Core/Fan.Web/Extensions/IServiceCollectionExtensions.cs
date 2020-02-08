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
        /// <summary>
        /// Adds services plugins depend on.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="hostingEnvironment"></param>
        /// <returns></returns>
        public static IServiceCollection AddPlugins(this IServiceCollection services, IWebHostEnvironment hostingEnvironment)
        {
            var sysPluginsDirs = Directory.GetDirectories(Path.Combine(hostingEnvironment.ContentRootPath, "SysPlugins"));
            var pluginsDirs = Directory.GetDirectories(Path.Combine(hostingEnvironment.ContentRootPath, "Plugins"));
            var totalDirs = new string[sysPluginsDirs.Length + pluginsDirs.Length];
            sysPluginsDirs.CopyTo(totalDirs, 0);
            pluginsDirs.CopyTo(totalDirs, sysPluginsDirs.Length);
            var binDir = Util.IsRunningFromTestHost() ? Environment.CurrentDirectory :
                Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            foreach (var dir in totalDirs)
            {
                try
                {
                    // get plugin.json
                    var pluginJson = Path.Combine(dir, PluginService.PLUGIN_MANIFEST);
                    if (!File.Exists(pluginJson))
                    {
                        throw new FanException($"The {PluginService.PLUGIN_MANIFEST} is not found.");
                    }

                    // load plugin dll
                    var pluginManifest = JsonConvert.DeserializeObject<PluginManifest>(File.ReadAllText(pluginJson));
                    var dllPath = Path.Combine(binDir, pluginManifest.GetDllFileName());                   
                    var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(dllPath);

                    // configure plugin
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

            // configure services
            foreach (var plugin in services.BuildServiceProvider().GetServices<Plugin>())
            {
                plugin.ConfigureServices(services);
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
            services.Configure<AppSettings>(options => appSettingsConfigSection.Bind(options));
            var appSettings = appSettingsConfigSection.Get<AppSettings>();
            if (appSettings.MediaStorageType == EMediaStorageType.AzureBlob)
                services.AddScoped<IStorageProvider, AzureBlobStorageProvider>();
            else
                services.AddScoped<IStorageProvider, FileSysStorageProvider>();

            return services;
        }
    }
}

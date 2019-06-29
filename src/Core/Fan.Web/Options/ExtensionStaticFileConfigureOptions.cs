using Fan.Helpers;
using Fan.Plugins;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;

namespace Fan.Web.Options
{
    /// <summary>
    /// Serves static files from extension projects.
    /// </summary>
    public class ExtensionStaticFileConfigureOptions : IPostConfigureOptions<StaticFileOptions>
    {
        private readonly IHostingEnvironment _environment;

        public ExtensionStaticFileConfigureOptions(IHostingEnvironment environment)
        {
            _environment = environment;
        }

        public void PostConfigure(string name, StaticFileOptions options)
        {
            // Basic initialization in case the options weren't initialized by any other component
            options.ContentTypeProvider = options.ContentTypeProvider ?? new FileExtensionContentTypeProvider();

            if (options.FileProvider == null && _environment.WebRootFileProvider == null)
            {
                throw new InvalidOperationException("Missing FileProvider.");
            }

            options.FileProvider = options.FileProvider ?? _environment.WebRootFileProvider;

            var pluginTypes = TypeFinder.Find<Plugin>();
            var fileProviders = new List<IFileProvider>
            {
                options.FileProvider
            };
            foreach (var pluginType in pluginTypes)
            {
                try
                {
                    fileProviders.Add(new ManifestEmbeddedFileProvider(pluginType.Assembly, "wwwroot"));
                }
                catch (Exception)
                {
                }
            }

            options.FileProvider = new CompositeFileProvider(fileProviders);
        }
    }
}

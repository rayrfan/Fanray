using Fan.Helpers;
using Fan.Plugins;
using Fan.Themes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;

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
            options = options ?? throw new ArgumentNullException(nameof(options));

            // Basic initialization in case the options weren't initialized by any other component
            options.ContentTypeProvider = options.ContentTypeProvider ?? new FileExtensionContentTypeProvider();
            if (options.FileProvider == null && _environment.WebRootFileProvider == null)
            {
                throw new InvalidOperationException("Missing FileProvider.");
            }

            options.FileProvider = options.FileProvider ?? _environment.WebRootFileProvider;

            var fileProviders = new List<IFileProvider>
            {
                options.FileProvider
            };

            var pluginTypes = TypeFinder.Find<Plugin>();
            var themeTypes = TypeFinder.Find<Theme>();
            var types = pluginTypes.Concat(themeTypes);
            foreach (var type in types)
            {
                try
                {
                    fileProviders.Add(new ManifestEmbeddedFileProvider(type.Assembly, "wwwroot"));
                }
                catch (Exception)
                {
                }
            }

            options.FileProvider = new CompositeFileProvider(fileProviders);
        }
    }
}

using Fan.Medias;
using Fan.Settings;
using Fan.Shortcodes;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
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

        /// <summary>
        /// Adds the <see cref="ShortcodeService"/> as a singleton.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddShortcodes(this IServiceCollection services)
        {
            var shortcodeService = new ShortcodeService();
            shortcodeService.Add<SourceCodeShortcode>(tag: "code");
            shortcodeService.Add<YouTubeShortcode>(tag: "youtube");
            services.AddSingleton<IShortcodeService>(shortcodeService);

            return services;
        }
    }
}

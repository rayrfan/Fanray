using Fan.Medias;
using Fan.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

namespace Fan.Web.Pages
{
    /// <summary>
    /// Used as a image handler to redirect to the actual image based on storage type and whether 
    /// it's dev or prd.
    /// </summary>
    public class ImageModel : PageModel
    {
        private readonly AppSettings _appSettings;
        private readonly IStorageProvider _storageProvider;

        public ImageModel(IServiceProvider serviceProvider, IStorageProvider storageProvider)
        {
            _appSettings = serviceProvider.GetService<IOptionsSnapshot<AppSettings>>().Value;
            _storageProvider = storageProvider;
        }

        /// <summary>
        /// Redirects to actual file url.
        /// </summary>
        /// <param name="appType"></param>
        /// <param name="size">Original or optimized</param>
        /// <param name="userId"></param>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public IActionResult OnGet(EAppType appType, EImageSize size, int userId, string year, string month, string fileName)
        {
            var endpoint = _storageProvider.StorageEndpoint;
            var container = _appSettings.MediaContainerName;
            var appName = appType.ToString().ToLowerInvariant();
            var sizeStr = size.ToString().ToLowerInvariant();

            return Redirect($"{endpoint}/{container}/{appName}/{sizeStr}/{userId}/{year}/{month}/{fileName}");
        }
    }
}
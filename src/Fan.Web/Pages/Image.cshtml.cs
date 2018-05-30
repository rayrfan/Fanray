using Fan.Medias;
using Fan.Settings;
using Microsoft.AspNetCore.Hosting;
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
        private readonly IHostingEnvironment _env;
        public ImageModel(IHostingEnvironment env, IServiceProvider serviceProvider)
        {
            _env = env;
            _appSettings = serviceProvider.GetService<IOptionsSnapshot<AppSettings>>().Value;
        }

        /// <summary>
        /// Redirects to actual file url based on storage type and env.
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
            var endpoint = ""; // for file sys this should be empty string
            if (_appSettings.MediaStorageType == EMediaStorageType.AzureBlob)
            {
                endpoint = _env.IsDevelopment() ?
                    $"http://127.0.0.1:10000/{_appSettings.StorageAccountName}" :
                    $"https://{_appSettings.StorageAccountName}.blob.core.windows.net";
            }
            var container = _appSettings.MediaContainerName;
            var appName = appType.ToString().ToLowerInvariant();
            var sizeStr = size.ToString().ToLowerInvariant();

            // e.g. blob storage url "https://fanray.blob.core.windows.net/media/blog/original/1/2018/05/test.png"
            // e.g. filesys url "/image/blog/original/1/2018/05/test.jpg"
            return Redirect($"{endpoint}/{container}/{appName}/{sizeStr}/{userId}/{year}/{month}/{fileName}");
        }
    }
}
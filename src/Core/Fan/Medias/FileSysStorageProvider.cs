using Fan.Settings;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Fan.Medias
{
    /// <summary>
    /// File system storage.
    /// </summary>
    /// user uploads file with an existing name, get a unique name
    /// the problem is olw, if user resizes an image, be aware olw sends it as new file
    /// also olw each time sends two copies of the file, orig and thumb
    public class FileSysStorageProvider : IStorageProvider
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly AppSettings _appSettings;
        private readonly HttpRequest _request;

        public FileSysStorageProvider(IHostingEnvironment env, IServiceProvider serviceProvider, 
            IHttpContextAccessor httpContextAccessor)
        {
            _hostingEnvironment = env;
            _appSettings = serviceProvider.GetService<IOptionsSnapshot<AppSettings>>().Value;
            _request = httpContextAccessor.HttpContext.Request;
        }

        // -------------------------------------------------------------------- public property

        /// <summary>
        /// The absolute URI endpoint to file, e.g. "https://localhost:44381" or "https://www.fanray.com".
        /// </summary>
        public string StorageEndpoint => $"{_request.Scheme}://{_request.Host}{_request.PathBase}";
        
        // -------------------------------------------------------------------- public method

        public async Task SaveFileAsync(byte[] source, string fileName, string path, char pathSeparator)
        {
            var root = _hostingEnvironment.WebRootPath;
            var container = _appSettings.MediaContainerName;
            var imgPath = path.Replace(pathSeparator, Path.DirectorySeparatorChar);
            var dirPath = $"{root}{Path.DirectorySeparatorChar}{container}{Path.DirectorySeparatorChar}{imgPath}";

            // make sure dir exists
            if (!Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);

            // combine dir and filename
            var filePath = Path.Combine(dirPath, fileName);

            // save source to file sys
            using (var fileStream = File.Create(filePath))
            using (var memStream = new MemoryStream(source))
            {
                await memStream.CopyToAsync(fileStream);
            }
        }

        /// <summary>
        /// Saves the file to server file system.
        /// </summary>
        public async Task SaveFileAsync(Stream source, string fileName, string path, char pathSeparator)
        {
            var root = _hostingEnvironment.WebRootPath;
            var container = _appSettings.MediaContainerName;
            var imgPath = path.Replace(pathSeparator, Path.DirectorySeparatorChar);
            var dirPath = $"{root}{Path.DirectorySeparatorChar}{container}{Path.DirectorySeparatorChar}{imgPath}";

            // make sure dir exists
            if (!Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);

            // combine dir and filename
            var filePath = Path.Combine(dirPath, fileName);

            // save source to file sys
            using (var fileStream = File.Create(filePath))
            {
                await source.CopyToAsync(fileStream);
            }
        }

        /// <summary>
        /// Deletes a file from storage.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public Task DeleteFileAsync(string fileName, string path, char pathSeparator)
        {
            var root = _hostingEnvironment.WebRootPath;
            var container = _appSettings.MediaContainerName;
            var imgPath = path.Replace(pathSeparator, Path.DirectorySeparatorChar);
            var filePath = $"{root}{Path.DirectorySeparatorChar}{container}{Path.DirectorySeparatorChar}{imgPath}{Path.DirectorySeparatorChar}{fileName}";

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            return Task.FromResult(0);
        }
    }
}

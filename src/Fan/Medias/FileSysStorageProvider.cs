using Fan.Settings;
using Microsoft.AspNetCore.Hosting;
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
        public FileSysStorageProvider(IHostingEnvironment env, IServiceProvider serviceProvider)
        {
            _hostingEnvironment = env;
            _appSettings = serviceProvider.GetService<IOptionsSnapshot<AppSettings>>().Value;
        }

        /// <summary>
        /// Returns unqiue file name after saveing file byte array to storage.
        /// </summary>
        /// <remarks>
        /// The storage type can be configured in appsettings.json. The file is stored like the following
        /// "container/appName/userId/year/month/fileName.ext".
        /// </remarks>
        /// <param name="source">The bytes of the file.</param>
        /// <param name="appId">Which app uploaded file.</param>
        /// <param name="userId">Who uploaded the file.</param>
        /// <param name="year">Upload year.</param>
        /// <param name="month">Upload month.</param>
        /// <param name="fileName">Slugged filename with ext.</param>
        public async Task<string> SaveFileAsync(byte[] source, EAppType appId, int userId,
                        DateTimeOffset uploadedOn, string fileName, EImageSize size)
        {
            var (fileNameUnique, filePath) = GetFileInfo(appId, userId, uploadedOn, fileName, size);

            // save source to file sys
            using (var targetStream = File.Create(filePath))
            using (var stream = new MemoryStream(source))
            {
                await stream.CopyToAsync(targetStream);
            }

            return fileNameUnique;
        }

        /// <summary>
        /// Returns unqiue file name after saveing file stream to storage.
        /// </summary>
        /// <remarks>
        /// The storage type can be configured in appsettings.json. The file is stored like the following
        /// "container/appName/userId/year/month/fileName.ext".
        /// </remarks>
        /// <param name="source">The stream of the file.</param>
        /// <param name="appId">Which app uploaded file.</param>
        /// <param name="userId">Who uploaded the file.</param>
        /// <param name="year">Upload year.</param>
        /// <param name="month">Upload month.</param>
        /// <param name="fileName">Slugged filename with ext.</param>
        public async Task<string> SaveFileAsync(Stream source, EAppType appId, int userId,
                        DateTimeOffset uploadedOn, string fileName, EImageSize size)
        {
            var (fileNameUnique, filePath) = GetFileInfo(appId, userId, uploadedOn, fileName, size);

            // save source to file sys
            using (var fileStream = File.Create(filePath))
            {
                await source.CopyToAsync(fileStream);
            }

            return fileNameUnique;
        }

        /// <summary>
        /// Returns unique file name and file path.
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="userId"></param>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="fileName">Slugged filename with ext.</param>
        /// <returns></returns>
        /// <remarks>
        /// If file with incoming filename already exists, this method appends a number to the filename,
        /// the number starts at 1.
        /// </remarks>
        private (string fileNameUnique, string filePath) GetFileInfo(EAppType appId, int userId, 
            DateTimeOffset uploadedOn, string fileName, EImageSize size)
        {
            // dir to save this file in e.g. "wwwroot\media\blog\optimized\1\2018\05"
            var root = _hostingEnvironment.WebRootPath;
            var container = _appSettings.MediaContainerName;
            var appName = appId.ToString().ToLowerInvariant();
            var qualityStr = size.ToString().ToLowerInvariant();
            var year = uploadedOn.Year.ToString();
            var month = uploadedOn.Month.ToString("d2");

            var dirPath = string.Format("{0}\\{1}\\{2}\\{3}\\{4}\\{5}\\{6}",
                root,
                container,
                appName,
                qualityStr,
                userId,
                year,
                month);

            // make sure dir exists
            if (!Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);

            // combine dir and filename
            var filePath = Path.Combine(dirPath, fileName);

            // make sure file is unique
            int i = 1;
            while (File.Exists(filePath))
            {
                fileName = fileName.Insert(fileName.LastIndexOf('.'), $"-{i}");
                filePath = Path.Combine(dirPath, fileName);
                i++;
            }

            return (fileNameUnique: fileName, filePath: filePath);
        }
    }
}

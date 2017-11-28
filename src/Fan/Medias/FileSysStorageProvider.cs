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
        /// Returns relative path to a file after saving it on the file system.
        /// </summary>
        /// <param name="userId">The id of the user who uploads.</param>
        /// <param name="fileName">Slugged filename with ext.</param>
        /// <param name="year">Upload year.</param>
        /// <param name="month">Upload month.</param>
        /// <param name="content">The content of file.</param>
        /// <param name="appId">Which app it uploaded it.</param>
        /// <returns></returns>
        public async Task<string> SaveFileAsync(int userId, string fileName, string year, string month, byte[] content, EAppType appId)
        {
            // app name
            var appName = appId.ToString().ToLowerInvariant();

            // dir to save this file in
            var dirPath = string.Format("{0}\\{1}\\{2}\\{3}\\{4}\\{5}",
                _hostingEnvironment.WebRootPath,
                _appSettings.MediaContainerName,
                appName,
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
            }

            // save file to file sys
            using (var targetStream = File.Create(filePath))
            using (MemoryStream stream = new MemoryStream(content))
            {
                await stream.CopyToAsync(targetStream);
            }

            // returns relative path
            return $"{_appSettings.MediaContainerName}/{appName}/{userId}/{year}/{month}/{fileName}";
        }
    }
}

using Microsoft.AspNetCore.Hosting;
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
        /// <summary>
        /// All files will be saved into a "media" folder right under wwwroot, such that a file 
        /// will have this url https://yoursite.com/media/2017/11/file-name.jpg
        /// </summary>
        public const string MEDIA_UPLOADS_FOLDER = "media";

        private readonly IHostingEnvironment _hostingEnvironment;
        public FileSysStorageProvider(IHostingEnvironment env)
        {
            _hostingEnvironment = env;
        }

        /// <summary>
        /// Returns relative path to a file after saving it on the file system.
        /// </summary>
        /// <param name="fileName">Slugged filename with ext.</param>
        /// <param name="year">Upload year.</param>
        /// <param name="month">Upload month.</param>
        /// <param name="content">The content of file.</param>
        /// <param name="appId">Which app it uploaded it.</param>
        /// <returns></returns>
        public async Task<string> SaveFileAsync(string fileName, string year, string month, byte[] content, EAppType appId)
        {
            // dir to save this file in
            var dirPath = string.Format("{0}\\{1}\\{2}\\{3}",
                Path.Combine(_hostingEnvironment.WebRootPath),
                MEDIA_UPLOADS_FOLDER,
                year,
                month);

            // make sure dir exists
            if (!Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);

            // combine dir and filename
            var filePath = Path.Combine(dirPath, fileName); 

            // make sure file is unique
            int i = 2;
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
            return $"{MEDIA_UPLOADS_FOLDER}/{year}/{month}/{fileName}";
        }
    }
}

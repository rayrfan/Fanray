using Fan.Exceptions;
using Fan.Helpers;
using Fan.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Fan.Medias
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// todo: track upload limit (6GB) in user profile
    /// </remarks>
    public class MediaService : IMediaService
    {
        /// <summary>
        /// Max len for a media filename is 128.
        /// </summary>
        public const int MEDIA_FILENAME_MAXLEN = 128;

        /// <summary>
        /// Folder to save uploaded media is called "uploads".
        /// </summary>
        public const string MEDIA_UPLOADS_FOLDER = "uploads";

        /// <summary>
        /// Accepted image types: .jpg .jpeg .png .gif
        /// </summary>
        /// <remarks>
        /// Getting this idea from WP https://en.support.wordpress.com/images/
        /// For accepted file types https://en.support.wordpress.com/accepted-filetypes/
        /// </remarks>
        public static readonly string[] Accepted_Image_Types = { ".jpg", ".jpeg", ".gif", ".png" };

        private readonly UserManager<User> _userManager;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IMediaRepository _mediaRepo;
        public MediaService(
            UserManager<User> userManager,
            IMediaRepository mediaRepo, 
            IHostingEnvironment env)
        {
            _userManager = userManager;
            _hostingEnvironment = env;
            _mediaRepo = mediaRepo;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="name">File name</param>
        /// <param name="content">File content</param>
        /// <returns></returns>
        public async Task<string> UploadMediaAsync(string userName, string name, byte[] content, EAppType appId)
        {
            // verify ext is supported
            string ext = Path.GetExtension(name);
            if (ext.IsNullOrEmpty() || !Accepted_Image_Types.Contains(ext, StringComparer.InvariantCultureIgnoreCase))
                throw new FanException("Upload image type is not supported.");

            // time
            var uploadedOn = DateTimeOffset.UtcNow;
            var year = uploadedOn.Year.ToString();
            var month = uploadedOn.Month.ToString("d2");

            // directory path to save this file in
            var dirPath = string.Format("{0}\\{1}\\{2}\\{3}",
                Path.Combine(_hostingEnvironment.WebRootPath),
                MEDIA_UPLOADS_FOLDER,
                year,
                month);

            if (!Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);

            // file path
            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(name);
            if (fileNameWithoutExt.Length > MEDIA_FILENAME_MAXLEN)
            {
                fileNameWithoutExt = fileNameWithoutExt.Substring(0, MEDIA_FILENAME_MAXLEN);
            }

            string slug = Util.FormatSlug(fileNameWithoutExt); // chinese fn ends up emtpy
            if (slug.IsNullOrEmpty())
            {
                slug = Util.RandomString(6);
            }

            string fileName = $"{slug}{ext}";
            var filePath = Path.Combine(dirPath, fileName); // C:\Fan.Web\wwwroot\uploads\2017\10\test-pic.jpg

            // user uploads file with an existing name, get a unique name
            // the problem is olw, if user resizes an image, be aware olw sends it as new file
            // also olw each time sends two copies of the file, orig and thumb
            int i = 2;
            while (File.Exists(filePath))
            {
                fileName = fileName.Insert(fileName.LastIndexOf('.'), $"-{i}");
                filePath = Path.Combine(dirPath, fileName);
            }

            // save file to file sys, always a new file
            using (var targetStream = File.Create(filePath))
            using (MemoryStream stream = new MemoryStream(content))
            {
                await stream.CopyToAsync(targetStream);
            }

            // find user
            var user = await _userManager.FindByNameAsync(userName);

            // save record to db
            var media = new Media
            {
                UserId = user.Id,
                AppId = appId,
                FileName = fileName,
                Title = fileNameWithoutExt,
                Description = fileNameWithoutExt,
                Length = content.LongLength,
                MediaType = EMediaType.Image,
                UploadedOn = uploadedOn,
            };
            await _mediaRepo.CreateAsync(media);

            // update user uploadLength
            user.UploadLength += content.LongLength;
            await _userManager.UpdateAsync(user);

            // a challenge here is that this returned url will be hardcoded into post
            // if user later switches to Blob Storage or CDN instead of file sys
            // all these post will break. If that happens, the easy remedy is keep 
            // existing copies of files where they were, not ideal.
            return $"{MEDIA_UPLOADS_FOLDER}/{year}/{month}/{fileName}";
        }

        public async Task<Media> UpdateMediaAsync(int id, string title, string description)
        {
            var media = await _mediaRepo.GetAsync(id);
            title = title.IsNullOrEmpty() ? "" : title;
            media.Title = title.Length > MEDIA_FILENAME_MAXLEN ?
             title.Substring(0, MEDIA_FILENAME_MAXLEN) : title;
            media.Description = description;
            return await _mediaRepo.UpdateAsync(media);
        }

    }
}

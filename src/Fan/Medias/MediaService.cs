using Fan.Exceptions;
using Fan.Helpers;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Fan.Medias
{
    /// <summary>
    /// The service manages media files.
    /// </summary>
    public class MediaService : IMediaService
    {
        /// <summary>
        /// Max len for a media filename is 128.
        /// </summary>
        public const int MEDIA_FILENAME_MAXLEN = 128;

        /// <summary>
        /// Accepted image types: .jpg .jpeg .png .gif
        /// </summary>
        /// <remarks>
        /// Getting this idea from WP https://en.support.wordpress.com/images/
        /// For accepted file types https://en.support.wordpress.com/accepted-filetypes/
        /// </remarks>
        public static readonly string[] Accepted_Image_Types = { ".jpg", ".jpeg", ".gif", ".png" };

        private readonly IStorageProvider _storageProvider;
        private readonly IMediaRepository _mediaRepo;
        public MediaService(IStorageProvider storageProvider, IMediaRepository mediaRepo)
        {
            _storageProvider = storageProvider;
            _mediaRepo = mediaRepo;
        }

        /// <summary>
        /// Returns media url after upload to storage.
        /// </summary>
        /// <param name="userId">Id of the user uploading the media.</param>
        /// <param name="fileName">File name with ext.</param>
        /// <param name="content">File content</param>
        /// <param name="appId">Which app it uploaded it.</param>
        /// <returns></returns>
        /// <remarks>
        /// Note: This method is optimized for metaweblog use with olw, other apps have totally 
        /// different file logic.
        /// 
        /// Depending on the storage provider, the returned media url could be relative path 
        /// (File Sys) or absolute path (Azure Blog).
        /// </remarks>
        public async Task<string> UploadMediaAsync(int userId, string fileName, byte[] content, EAppType appId, EUploadedFrom uploadFrom)
        {
            // verify ext is supported
            var ext = Path.GetExtension(fileName);
            if (ext.IsNullOrEmpty() || !Accepted_Image_Types.Contains(ext, StringComparer.InvariantCultureIgnoreCase))
                throw new FanException("Upload file type is not supported.");

            // time
            var uploadedOn = DateTimeOffset.UtcNow;
            var year = uploadedOn.Year.ToString();
            var month = uploadedOn.Month.ToString("d2");

            // make sure file name is not too long
            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
            if (fileNameWithoutExt.Length > MEDIA_FILENAME_MAXLEN)
            {
                fileNameWithoutExt = fileNameWithoutExt.Substring(0, MEDIA_FILENAME_MAXLEN);
            }

            // there is a quirk file uploaded from olw had "_2" suffixed to the name
            if (uploadFrom == EUploadedFrom.MetaWeblog && fileNameWithoutExt.EndsWith("_2"))
            {
                fileNameWithoutExt = fileNameWithoutExt.Remove(fileNameWithoutExt.Length - 2);
            }

            // slug file name
            // chinese fn ends up emtpy and the thumb file with chinese fn ends up with only "thumb"
            var slug = Util.FormatSlug(fileNameWithoutExt); 
            if (slug.IsNullOrEmpty())
            {
                slug = Util.RandomString(6);
            }
            else if (uploadFrom == EUploadedFrom.MetaWeblog && slug == "thumb")
            {
                slug = string.Concat(Util.RandomString(6), "_thumb");
            }
            string fileNameSlugged = $"{slug}{ext}";

            // save file to storage and get back file path
            var filePath = await _storageProvider.SaveFileAsync(userId, fileNameSlugged, year, month, content, EAppType.Blog);

            // encode filename 
            var fileNameEncoded = WebUtility.HtmlEncode(fileNameWithoutExt);

            // since file name could have been updated for uniqueness
            var start = filePath.LastIndexOf('/') + 1;
            var uniqueFileName = filePath.Substring(start, filePath.Length - start);

            // save record to db
            var media = new Media
            {
                UserId = userId,
                AppId = appId,
                FileName = uniqueFileName, // unique filename from storage provider
                Title = fileNameEncoded,
                Description = fileNameEncoded,
                Length = content.LongLength,
                MediaType = EMediaType.Image,
                UploadedOn = uploadedOn,
                UploadedFrom = uploadFrom,
            };
            await _mediaRepo.CreateAsync(media);

            return filePath;
        }

        public async Task<Media> UpdateMediaAsync(int id, string title, string description)
        {
            var media = await _mediaRepo.GetAsync(id);
            title = title.IsNullOrEmpty() ? "" : title;
            media.Title = title.Length > MEDIA_FILENAME_MAXLEN ?
             title.Substring(0, MEDIA_FILENAME_MAXLEN) : title;
            media.Description = description;

            await _mediaRepo.UpdateAsync(media);
            return media;
        }
    }
}

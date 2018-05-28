using Fan.Exceptions;
using Fan.Helpers;
using System;
using System.Collections.Generic;
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
        /// This will prefix the image url to trigger <see cref="Image.cshtml"/>.
        /// </summary>
        public const string IMAGE_HANDLER_PATH = "/image";

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
        /// Returns image url after uploading image byte array to storage.
        /// </summary>
        /// <param name="userId">Id of the user uploading the media.</param>
        /// <param name="fileName">File name with ext.</param>
        /// <param name="source">File content</param>
        /// <param name="appId">Which app it uploaded it.</param>
        /// <returns></returns>
        /// <remarks>
        /// This is currently only used by olw and is optimized for metaweblog use with olw, 
        /// other apps have totally different file logic.  Image is not resized.
        /// </remarks>
        public async Task<string> UploadImageAsync(byte[] source, EAppType appId, int userId, string fileName, EUploadedFrom uploadFrom)
        {
            // slugged and encoded file names
            var (fileNameSlugged, fileNameEncoded) = GetFileNames(fileName, uploadFrom);

            // time
            var uploadedOn = DateTimeOffset.UtcNow;
            var year = uploadedOn.Year.ToString();
            var month = uploadedOn.Month.ToString("d2");

            // save to storage
            string uniqueFileName = await _storageProvider.SaveFileAsync(source, EAppType.Blog, userId, year, month, fileNameSlugged);

            // create record in db
            await CreateMediaAsync(userId, appId, uniqueFileName, fileNameEncoded, source.LongLength, uploadedOn, uploadFrom);

            return $"{IMAGE_HANDLER_PATH}/{appId.ToString().ToLower()}/{userId}/{year}/{month}/{uniqueFileName}";
        }

        /// <summary>
        /// Returns image url after uploading and resizeing image stream to storage.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="appId"></param>
        /// <param name="userId"></param>
        /// <param name="fileName"></param>
        /// <param name="uploadFrom"></param>
        /// <returns></returns>
        public async Task<string> UploadImageAsync(Stream source, EAppType appId, int userId, string fileName, EUploadedFrom uploadFrom)
        {
            // slugged and encoded file names
            var (fileNameSlugged, fileNameEncoded) = GetFileNames(fileName, uploadFrom);

            // time
            var uploadedOn = DateTimeOffset.UtcNow;
            var year = uploadedOn.Year.ToString();
            var month = uploadedOn.Month.ToString("d2");

            // save to storage
            string uniqueFileName = await _storageProvider.SaveFileAsync(source, EAppType.Blog, userId, year, month, fileNameSlugged);

            // create record in db
            await CreateMediaAsync(userId, appId, uniqueFileName, fileNameEncoded, source.Length, uploadedOn, uploadFrom);

            return $"{IMAGE_HANDLER_PATH}/{appId.ToString().ToLower()}/{userId}/{year}/{month}/{uniqueFileName}";
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

        public async Task<List<Media>> GetMediasAsync(EMediaType mediaType, int pageNumber = 1, int pageSize = 50)
        {
            return await _mediaRepo.GetMediasAsync(mediaType, pageNumber, pageSize);
        }

        /// <summary>
        /// Returns slugged file name.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="uploadFrom"></param>
        /// <returns></returns>
        private (string fileNameSlugged, string fileNameEncoded) GetFileNames(string fileName, EUploadedFrom uploadFrom)
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

            var fileNameSlugged = $"{slug}{ext}";
            var fileNameEncoded = WebUtility.HtmlEncode(fileNameWithoutExt);


            return (fileNameSlugged: fileNameSlugged, fileNameEncoded: fileNameEncoded);
        }

        private async Task CreateMediaAsync(int userId, EAppType appId, string uniqueFileName, string fileNameEncoded, 
            long length, DateTimeOffset uploadedOn, EUploadedFrom uploadFrom)
        {
            // save record to db
            var media = new Media
            {
                UserId = userId,
                AppId = appId,
                FileName = uniqueFileName, // unique filename from storage provider
                Title = fileNameEncoded, // original filename
                Description = null,
                Length = length,
                MediaType = EMediaType.Image,
                UploadedOn = uploadedOn,
                UploadedFrom = uploadFrom,
            };
            await _mediaRepo.CreateAsync(media);
        }
    }
}

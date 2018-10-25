using Fan.Blog.Enums;
using Fan.Blog.Services.Interfaces;
using Fan.Helpers;
using Fan.Medias;
using Fan.Settings;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Fan.Blog.Services
{
    /// <summary>
    /// The blog image service.
    /// </summary>
    public class ImageService : IImageService
    {
        private readonly IMediaService _mediaSvc;
        private readonly IStorageProvider _storageProvider;
        private readonly AppSettings _appSettings;

        public ImageService(IMediaService mediaSvc,
                            IStorageProvider storageProvider,
                            IOptionsSnapshot<AppSettings> appSettings)
        {
            _mediaSvc = mediaSvc;
            _storageProvider = storageProvider;
            _appSettings = appSettings.Value;
        }

        // -------------------------------------------------------------------- const and config

        /// <summary>
        /// "Blog"
        /// </summary>
        public const string BLOG_APP_NAME = "Blog";

        /// <summary>
        /// Blog accepted image types: .jpg .jpeg .png .gif
        /// </summary>
        /// <remarks>
        /// Got the idea from WP https://en.support.wordpress.com/images/
        /// For accepted file types https://en.support.wordpress.com/accepted-filetypes/
        /// </remarks>
        public static readonly string[] Accepted_Image_Types = { ".jpg", ".jpeg", ".gif", ".png" };

        /// <summary>
        /// The separator used in image paths is '/'.
        /// </summary>
        /// <remarks>
        /// All <see cref="IStorageProvider"/> implementations should take this separator and replace 
        /// it with your specific one.
        /// </remarks>
        public const char IMAGE_PATH_SEPARATOR = '/';

        /// <summary>
        /// Large image size 1200 pixel.
        /// </summary>
        public const int LARGE_IMG_SIZE = 1200;

        /// <summary>
        /// Medium image size 800 pixel.
        /// </summary>
        public const int MEDIUM_IMG_SIZE = 800;

        /// <summary>
        /// Small image size 400 pixel.
        /// </summary>
        public const int SMALL_IMG_SIZE = 400;

        /// <summary>
        /// The different image resizes per image upload.
        /// </summary>
        /// <param name="uploadedOn"></param>
        /// <returns></returns>
        public static List<ImageResizeInfo> GetImageResizeList(DateTimeOffset uploadedOn)
        {
            return new List<ImageResizeInfo> {
                new ImageResizeInfo {
                    TargetSize = int.MaxValue,
                    Path = GetImagePath(uploadedOn, EImageSize.Original),
                    PathSeparator = IMAGE_PATH_SEPARATOR,
                },
                new ImageResizeInfo {
                    TargetSize = LARGE_IMG_SIZE,
                    Path = GetImagePath(uploadedOn, EImageSize.Large),
                    PathSeparator = IMAGE_PATH_SEPARATOR,
                },
                new ImageResizeInfo {
                    TargetSize = MEDIUM_IMG_SIZE,
                    Path = GetImagePath(uploadedOn, EImageSize.Medium),
                    PathSeparator = IMAGE_PATH_SEPARATOR,
                },
                new ImageResizeInfo {
                    TargetSize = SMALL_IMG_SIZE,
                    Path = GetImagePath(uploadedOn, EImageSize.Small),
                    PathSeparator = IMAGE_PATH_SEPARATOR,
                },
            };
        }

        /// <summary>
        /// Returns the stored image path, "{app}/{year}/{month}" or "{app}/{year}/{month}/{sizePath}".
        /// </summary>
        /// <param name="uploadedOn"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static string GetImagePath(DateTimeOffset uploadedOn, EImageSize size)
        {
            var app = BLOG_APP_NAME.ToLowerInvariant();
            var year = uploadedOn.Year.ToString();
            var month = uploadedOn.Month.ToString("d2");
            var sizePath = "";

            switch (size)
            {
                case EImageSize.Large:
                    sizePath = "lg";
                    break;
                case EImageSize.Medium:
                    sizePath = "md";
                    break;
                case EImageSize.Small:
                    sizePath = "sm";
                    break;
                default:
                    sizePath = null;
                    break;
            }

            return size == EImageSize.Original ? $"{app}/{year}/{month}" : $"{app}/{year}/{month}/{sizePath}";
        }

        // -------------------------------------------------------------------- public methods

        /// <summary>
        /// Deletes an image from data source and storage.
        /// </summary>
        /// <param name="mediaId"></param>
        /// <returns></returns>
        public async Task DeleteAsync(int mediaId)
        {
            var media = await _mediaSvc.GetMediaAsync(mediaId);
            var resizes = GetImageResizeList(media.UploadedOn);
            var resizeCount = media.ResizeCount; // how many files to delete

            // delete file from storage
            await DeleteImageFileAsync(media, EImageSize.Original);
            if (resizeCount == 3)
            {
                await DeleteImageFileAsync(media, EImageSize.Small);
                await DeleteImageFileAsync(media, EImageSize.Medium);
                await DeleteImageFileAsync(media, EImageSize.Large);
            }
            else if (resizeCount == 2)
            {
                await DeleteImageFileAsync(media, EImageSize.Small);
                await DeleteImageFileAsync(media, EImageSize.Medium);
            }
            else if (resizeCount == 1)
            {
                await DeleteImageFileAsync(media, EImageSize.Small);
            }

            // delete from db
            await _mediaSvc.DeleteMediaAsync(mediaId);
        }

        /// <summary>
        /// Returns absolute url to an image.
        /// </summary>
        /// <remarks>
        /// Based on the resize count, the url returned could be original or one of the resized image.
        /// </remarks>
        /// <param name="media">The media record representing the image.</param>
        /// <param name="size">The image size.</param>
        /// <returns></returns>
        public string GetAbsoluteUrl(Media media, EImageSize size)
        {
            var endpoint = _storageProvider.StorageEndpoint;
            var container = endpoint.EndsWith('/') ? _appSettings.MediaContainerName : $"/{_appSettings.MediaContainerName}";

            if ((size == EImageSize.Original || media.ResizeCount <= 0) ||
                (media.ResizeCount == 1 && size != EImageSize.Small) ||
                (media.ResizeCount == 2 && size == EImageSize.Large))
            {
                size = EImageSize.Original;
            }

            var imagePath = GetImagePath(media.UploadedOn, size);
            var fileName = media.FileName;

            return $"{endpoint}{container}/{imagePath}/{fileName}";
        }

        /// <summary>
        /// Uploads image.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="userId"></param>
        /// <param name="fileName"></param>
        /// <param name="contentType">e.g. "image/jpeg"</param>
        /// <param name="uploadFrom"></param>
        /// <returns></returns>
        public async Task<Media> UploadAsync(Stream source, int userId, string fileName, string contentType,
            EUploadedFrom uploadFrom)
        {
            // check if file type is supported
            var ext = Path.GetExtension(fileName).ToLower();
            var ctype = "." + contentType.Substring(contentType.LastIndexOf("/") + 1).ToLower();
            if (ext.IsNullOrEmpty() || !Accepted_Image_Types.Contains(ext) || !Accepted_Image_Types.Contains(ctype))
            {
                throw new NotSupportedException("Upload file type is not supported.");
            }

            // uploadedOn 
            var uploadedOn = DateTimeOffset.UtcNow;

            // get the slugged filename and title from original filename
            var (fileNameSlugged, title) = ProcessFileName(fileName, uploadFrom);

            // get unique filename
            var uniqueFileName = await GetUniqueFileNameAsync(fileNameSlugged, uploadedOn);

            // get image resizes
            var resizes = GetImageResizeList(uploadedOn);

            return await _mediaSvc.UploadImageAsync(source, resizes, uniqueFileName, contentType, title,
                uploadedOn, EAppType.Blog, userId, uploadFrom);
        }

        // -------------------------------------------------------------------- private methods

        /// <summary>
        /// Deletes an image file from storage.
        /// </summary>
        /// <param name="media"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        private async Task DeleteImageFileAsync(Media media, EImageSize size)
        {
            var path = GetImagePath(media.UploadedOn, size);
            await _storageProvider.DeleteFileAsync(media.FileName, path, IMAGE_PATH_SEPARATOR);
        }

        /// <summary>
        /// Takes the original filename and returns a slugged filename and title attribute.
        /// </summary>
        /// <remarks>
        /// If the filename is too long it shorten it. Then it generates a slugged filename which 
        /// is hyphen separeated value for english original filenames, a random string value for 
        /// non-english filenames.  The title attribute is original filename html-encoded for safe
        /// display.
        /// </remarks>
        /// <param name="fileNameOrig">Original filename user is uploading.</param>
        /// <param name="uploadFrom">This is used solely because of olw quirks I have to handle.</param>
        /// <returns></returns>
        private (string fileNameSlugged, string title) ProcessFileName(string fileNameOrig, EUploadedFrom uploadFrom)
        {
            // extra filename without ext, note this will also remove the extra path info from OLW
            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileNameOrig);

            // make sure file name is not too long
            if (fileNameWithoutExt.Length > MediaService.MEDIA_FILENAME_MAXLEN)
            {
                fileNameWithoutExt = fileNameWithoutExt.Substring(0, MediaService.MEDIA_FILENAME_MAXLEN);
            }

            // there is a quirk file uploaded from olw had "_2" suffixed to the name
            if (uploadFrom == EUploadedFrom.MetaWeblog && fileNameWithoutExt.EndsWith("_2"))
            {
                fileNameWithoutExt = fileNameWithoutExt.Remove(fileNameWithoutExt.Length - 2);
            }

            // slug file name
            var slug = Util.FormatSlug(fileNameWithoutExt);
            if (slug.IsNullOrEmpty()) // slug may end up empty
            {
                slug = Util.RandomString(6);
            }
            else if (uploadFrom == EUploadedFrom.MetaWeblog && slug == "thumb") // or may end up with only "thumb" for olw
            {
                slug = string.Concat(Util.RandomString(6), "_thumb");
            }

            var ext = Path.GetExtension(fileNameOrig).ToLower();
            var fileNameSlugged = $"{slug}{ext}";
            var fileNameEncoded = WebUtility.HtmlEncode(fileNameWithoutExt);

            return (fileNameSlugged: fileNameSlugged, title: fileNameEncoded);
        }

        /// <summary>
        /// Returns a unique filename after checking datasource to see if the filename exists already.
        /// </summary>
        /// <param name="uploadedOn"></param>
        /// <param name="fileNameSlugged"></param>
        /// <returns></returns>
        private async Task<string> GetUniqueFileNameAsync(string fileNameSlugged, DateTimeOffset uploadedOn)
        {
            int i = 1;
            while (await _mediaSvc.ExistsAsync(m => m.AppType == EAppType.Blog &&
                                                    m.UploadedOn.Year == uploadedOn.Year &&
                                                    m.UploadedOn.Month == uploadedOn.Month &&
                                                    m.FileName.Equals(fileNameSlugged)))
            {
                var lookUp = ".";
                var replace = $"-{i}.";
                if (i > 1)
                {
                    int j = i - 1;
                    lookUp = $"-{j}.";
                }

                fileNameSlugged = fileNameSlugged.Replace(lookUp, replace);
                i++;
            }

            return fileNameSlugged;
        }
    }
}

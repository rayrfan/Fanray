using Fan.Helpers;
using Fan.Settings;
using ImageMagick;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Fan.Medias
{
    /// <summary>
    /// The media service manages media files: resizes image file, generates unique filename, passes file to storage, 
    /// generates handler url.  
    /// </summary>
    /// <remarks>
    /// See Media model class and admin Media.cshtml page for more information.
    /// </remarks>
    public class MediaService : IMediaService
    {
        /// <summary>
        /// If image file size exceeds 5MB then use a lower quality.
        /// </summary>
        public const long IMAGE_MAX_LEN = 5 * ByteSize.BytesInMegaByte;

        /// <summary>
        /// Max len for a media filename is 128.
        /// </summary>
        public const int MEDIA_FILENAME_MAXLEN = 128;

        private readonly IStorageProvider _storageProvider;
        private readonly AppSettings _appSettings;
        private readonly IMediaRepository _mediaRepo;

        // -------------------------------------------------------------------- constructor

        public MediaService(IStorageProvider storageProvider,
            IOptionsSnapshot<AppSettings> settings,
            IMediaRepository mediaRepo)
        {
            _storageProvider = storageProvider;
            _appSettings = settings.Value;
            _mediaRepo = mediaRepo;
        }

        // -------------------------------------------------------------------- public methods

        /// <summary>
        /// Returns a list of <see cref="Media"/> records based on search critria.
        /// </summary>
        /// <param name="mediaType"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async Task<List<Media>> GetMediasAsync(EMediaType mediaType, int pageNumber, int pageSize)
        {
            return await _mediaRepo.GetMediasAsync(mediaType, pageNumber, pageSize);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="title"></param>
        /// <param name="description"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Uploads image by resizing and storing it.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="resizes"></param>
        /// <param name="fileName"></param>
        /// <param name="contentType"></param>
        /// <param name="uploadedOn"></param>
        /// <param name="appType"></param>
        /// <param name="userId"></param>
        /// <param name="uploadFrom"></param>
        /// <returns></returns>
        /// <remarks>
        /// https://github.com/dlemstra/Magick.NET/blob/master/Documentation/ResizeImage.md
        /// TODO currently resized gif size is bigger than original.
        /// </remarks>
        public async Task<Media> UploadImageAsync(Stream source,
                                                  List<ImageResizeInfo> resizes,
                                                  string fileName,
                                                  string contentType,
                                                  string title,
                                                  DateTimeOffset uploadedOn,
                                                  EAppType appType,
                                                  int userId,
                                                  EUploadedFrom uploadFrom = EUploadedFrom.Browser)
        {
            int widthOrig;
            int heightOrig;
            int resizeCount = -1;
            if (contentType.Equals("image/gif"))
            {
                using (var imageColl = new MagickImageCollection(source))
                {
                    widthOrig = imageColl[0].Width;
                    heightOrig = imageColl[0].Height;
                    imageColl.Coalesce();

                    // resize and store
                    foreach (var resize in resizes)
                    {
                        // only resize when either MaxValue which mean original size 
                        // or image size is greater than what's being asked for
                        if (resize.Pixel == int.MaxValue || widthOrig > resize.Pixel)
                        {
                            resizeCount++;

                            foreach (MagickImage image in imageColl)
                            {
                                var (width, height) = GetNewSize(widthOrig, heightOrig, resize.Pixel);

                                //image.Quality = 75;
                                image.Resize(width, height);
                            }

                            using (var memStream = new MemoryStream())
                            {
                                imageColl.Write(memStream);
                                memStream.Position = 0;

                                await _storageProvider.SaveFileAsync(memStream, resize, fileName);
                            }
                        }
                    }
                }
            }
            else
            {
                using (var image = new MagickImage(source))
                {
                    widthOrig = image.Width;
                    heightOrig = image.Height;
                    //int resizeCount = -1;

                    // resize and store
                    foreach (var resize in resizes)
                    {
                        // only resize when either MaxValue which mean original size 
                        // or image size is greater than what's being asked for
                        if (resize.Pixel == int.MaxValue || widthOrig > resize.Pixel)
                        {
                            resizeCount++;

                            using (var memStream = new MemoryStream())
                            {
                                var (width, height) = GetNewSize(widthOrig, heightOrig, resize.Pixel);

                                image.Quality = 75;
                                image.Resize(new MagickGeometry(width, height));
                                image.Write(memStream);
                                memStream.Position = 0;

                                await _storageProvider.SaveFileAsync(memStream, resize, fileName);
                            }
                        }
                    }
                }
            }

            // create record in db
            var media = new Media
            {
                UserId = userId,
                AppType = appType,
                FileName = fileName,
                Title = title,
                Description = null,
                Length = source.Length,
                MediaType = EMediaType.Image,
                UploadedOn = uploadedOn,
                UploadedFrom = uploadFrom,
                Width = widthOrig,
                Height = heightOrig,
                Caption = title,
                ContentType = contentType,
                Alt = title,
                ResizeCount = resizeCount,
            };

            await _mediaRepo.CreateAsync(media);

            return media;
        }

        // -------------------------------------------------------------------- private

        /// <summary>
        /// Return new width and height to be resized to given the original image width and height and size pixel.
        /// </summary>
        /// <param name="imageWidth"></param>
        /// <param name="imageHeight"></param>
        /// <param name="pixel"></param>
        /// <returns></returns>
        private (int width, int height) GetNewSize(int imageWidth, int imageHeight, int pixel)
        {
            int width = 0;
            int height = 0;

            if (imageWidth == imageHeight) // square
            {
                width = height = imageWidth;
            }
            else if (imageWidth > imageHeight) // horizontal 
            {
                var ratio = imageWidth / imageHeight;
                if (imageWidth <= pixel) // actual image is smaller, no need to change width and height
                {
                    width = imageWidth;
                    height = imageHeight;
                }
                else
                {
                    width = pixel;
                    height = width * ratio;
                }
            }
            else // vertial
            {
                var ratio = imageHeight / imageWidth;
                if (imageHeight <= pixel) // actual image is smaller, no need to change width and height
                {
                    width = imageWidth;
                    height = imageHeight;
                }
                else
                {
                    height = pixel;
                    width = height * ratio;
                }
            }

            return (width, height);
        }
    }
}
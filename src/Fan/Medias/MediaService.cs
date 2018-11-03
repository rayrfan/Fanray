using Fan.Exceptions;
using Fan.Helpers;
using Fan.Settings;
using ImageMagick;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
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
        private readonly IStorageProvider _storageProvider;
        private readonly AppSettings _appSettings;
        private readonly IMediaRepository _mediaRepo;

        public MediaService(IStorageProvider storageProvider,
            IOptionsSnapshot<AppSettings> settings,
            IMediaRepository mediaRepo)
        {
            _storageProvider = storageProvider;
            _appSettings = settings.Value;
            _mediaRepo = mediaRepo;
        }

        // -------------------------------------------------------------------- const

        /// <summary>
        /// Max len for a media filename is 128.
        /// </summary>
        public const int MEDIA_FILENAME_MAXLEN = 128;

        // -------------------------------------------------------------------- public methods

        /// <summary>
        /// Deletes a media by id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task DeleteMediaAsync(int id)
        {
            await _mediaRepo.DeleteAsync(id);
        }

        /// <summary>
        /// Returns true if media with the search criteria exists, false otherwise.
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public async Task<bool> ExistsAsync(Expression<Func<Media, bool>> predicate)
        {
            return (await _mediaRepo.FindAsync(predicate)).Count() > 0;
        }

        /// <summary>
        /// Returns list of media with the search criteria.
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Media>> FindAsync(Expression<Func<Media, bool>> predicate)
        {
            return await _mediaRepo.FindAsync(predicate);
        }

        /// <summary>
        /// Returns the media by id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Media> GetMediaAsync(int id)
        {
            return await _mediaRepo.GetAsync(id);
        }

        /// <summary>
        /// Returns a list of <see cref="Media"/> based on media type page number and page size, 
        /// or empty list if no records found; and total count of medias for this media type.
        /// </summary>
        /// <param name="mediaType"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async Task<(List<Media> medias, int count)> GetMediasAsync(EMediaType mediaType, int pageNumber, int pageSize)
        {
            return await _mediaRepo.GetMediasAsync(mediaType, pageNumber, pageSize);
        }

        /// <summary>
        /// Updates media title and description.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="title"></param>
        /// <param name="caption"></param>
        /// <param name="alt"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        public async Task<Media> UpdateMediaAsync(int id,
            string title,
            string caption,
            string alt,
            string description)
        {
            var media = await _mediaRepo.GetAsync(id);

            title = title.IsNullOrEmpty() ? "" : title;
            media.Title = title.Length > MEDIA_FILENAME_MAXLEN ? title.Substring(0, MEDIA_FILENAME_MAXLEN) : title;
            media.Caption = caption;
            media.Alt = alt;
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
            int resizeCount = 0;

            if (contentType.Equals("image/gif"))
            {
                using (var imageColl = new MagickImageCollection(source))
                {
                    widthOrig = imageColl[0].Width;
                    heightOrig = imageColl[0].Height;

                    // currently for gif I only save original so there is no resizing
                    // TODO: with ImageMagick resizing a gif take a long time 
                    // plus the resized gif has a larger file size than original
                    // I tried limit gif length to 800px, but even resizing to small has these issues

                    // resize and store
                    foreach (var resize in resizes)
                    {
                        // save original without resizing
                        if (resize.TargetSize == int.MaxValue)
                        {
                            source.Position = 0;
                            await _storageProvider.SaveFileAsync(source, fileName, resize.Path, resize.PathSeparator);
                        }
                        //else if (Math.Max(widthOrig, heightOrig) > resize.TargetSize)
                        //{
                        //    resizeCount++;
                        //    var (width, height) = GetNewSize(widthOrig, heightOrig, resize.TargetSize);

                        //    imageColl.Coalesce();
                        //    foreach (var image in imageColl)
                        //    {
                        //        var colors = image.TotalColors;
                        //        image.Resize(width, height); // resize will make # of colors higher
                        //        image.Quantize(new QuantizeSettings
                        //        {
                        //            Colors = colors, // set it back to the smaller original colors
                        //            DitherMethod = DitherMethod.No
                        //        });
                        //    }

                        //    imageColl.Optimize();
                        //    await _storageProvider.SaveFileAsync(imageColl.ToByteArray(), fileName, resize.Path, resize.PathSeparator);
                        //}
                    }
                }
            }
            else if (contentType.Equals("image/png"))
            {
                using (var image = new MagickImage(source))
                {
                    widthOrig = image.Width;
                    heightOrig = image.Height;

                    foreach (var resize in resizes)
                    {
                        // save original without resizing for png
                        if (resize.TargetSize == int.MaxValue)
                        {
                            source.Position = 0;
                            await _storageProvider.SaveFileAsync(source, fileName, resize.Path, resize.PathSeparator);
                        }
                        else if (Math.Max(widthOrig, heightOrig) > resize.TargetSize)
                        {
                            resizeCount++;
                            var (width, height) = GetNewSize(widthOrig, heightOrig, resize.TargetSize);
                            //image.Quality = 75; // does not seem to affect output file size, so commented out
                            image.Resize(width, height);

                            await _storageProvider.SaveFileAsync(image.ToByteArray(), fileName, resize.Path, resize.PathSeparator);
                        }
                    }
                }
            }
            else // jpg
            {
                using (var image = new MagickImage(source))
                {
                    widthOrig = image.Width;
                    heightOrig = image.Height;

                    foreach (var resize in resizes)
                    {
                        if (resize.TargetSize == int.MaxValue || Math.Max(widthOrig, heightOrig) > resize.TargetSize)
                        {
                            if (resize.TargetSize != int.MaxValue) resizeCount++;
                            var (width, height) = GetNewSize(widthOrig, heightOrig, resize.TargetSize);
                            // setting the Quality is needed for having it here makes a difference for a smaller output file size!
                            image.Quality = 85; // 75 is default
                            image.Resize(width, height);

                            await _storageProvider.SaveFileAsync(image.ToByteArray(), fileName, resize.Path, resize.PathSeparator);
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
        /// Returns new width and height to be resized while keeping aspect ratio.
        /// </summary>
        /// <param name="origWidth">The original image width.</param>
        /// <param name="origHeight">The original image height.</param>
        /// <param name="targetSize">The target image size.</param>
        /// <returns></returns>
        /// <remarks>
        /// Currently it calculates based on the longest side of either width or height.
        /// I may want to do based on width or height.
        /// </remarks>
        private (int width, int height) GetNewSize(int origWidth, int origHeight, int targetSize)
        {
            if (targetSize <= 0 || targetSize == int.MaxValue)
                return (origWidth, origHeight);

            int width = 0;
            int height = 0;

            if (origHeight > origWidth) // portrait
            {
                width = origWidth * (targetSize / origHeight);
                height = targetSize;
            }
            else // square or landscape
            {
                width = targetSize;
                height = origHeight * (targetSize / origWidth);
            }

            return (width, height);
        }
    }
}
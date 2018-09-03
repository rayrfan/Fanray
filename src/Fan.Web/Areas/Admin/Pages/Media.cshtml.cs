using Fan.Blog.Enums;
using Fan.Blog.Services;
using Fan.Medias;
using Fan.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Fan.Web.Pages.Admin
{
    public class MediaModel : PageModel
    {
        private readonly IBlogService _blogSvc;
        private readonly IMediaService _mediaSvc;
        private readonly UserManager<User> _userManager;

        public MediaModel(
            IBlogService blogSvc,
            IMediaService mediaSvc,
            UserManager<User> userManager)
        {
            _blogSvc = blogSvc;
            _mediaSvc = mediaSvc;
            _userManager = userManager;
        }

        // -------------------------------------------------------------------- Helper Classes

        public class ImageVM : Media
        {
            public string FileType { get; set; }
            public string UploadDate { get; set; }
            public string UploadVia { get; set; }
            /// <summary>
            /// The gallery image dialog shows small image as thumbs.
            /// </summary>
            public string UrlSmall { get; set; }
            /// <summary>
            /// The composer inserts medium image.
            /// </summary>
            public string UrlMedium { get; set; }
            /// <summary>
            /// The gallery image dialog preview shows the large image.
            /// </summary>
            public string UrlLarge { get; set; }
            /// <summary>
            /// The gallery image dialog sidebar shows the original url.
            /// </summary>
            public string UrlOriginal { get; set; }
        }

        // -------------------------------------------------------------------- consts & properties

        /// <summary>
        /// Display 100 images at a time.
        /// </summary>
        public const int PAGE_SIZE = 100;

        /// <summary>
        /// Total number of images.
        /// </summary>
        public int ImageCount { get; set; }

        /// <summary>
        /// The json data to bootstrap page initially.
        /// </summary>
        public string Data { get; private set; }

        // -------------------------------------------------------------------- public methods

        /// <summary>
        /// GET bootstrap initial page with json data.
        /// </summary>
        /// <returns></returns>
        public async Task OnGetAsync()
        {
            var (medias, count) = await GetImageVMsAsync(1);
            ImageCount = count;
            Data = JsonConvert.SerializeObject(medias);
        }

        /// <summary>
        /// Ajax GET
        /// </summary>
        /// <param name="pageNumber"></param>
        /// <returns></returns>
        public async Task<JsonResult> OnGetImagesAsync(int pageNumber = 1)
        {
            var (medias, count) = await GetImageVMsAsync(pageNumber);
            return new JsonResult(medias);
        }

        /// <summary>
        /// Uploads images and returns urls to optimized or original if optimized is not available.
        /// </summary>
        /// <param name="images"></param>
        /// <remarks>
        /// After uploads are done, it calls and return <see cref="GetImageListVMAsync"/>, this will
        /// refresh the grid.
        /// TODO better way is to return uploaded image urls only, let client append url to grid etc.
        /// </remarks>
        /// <returns><see cref="ImageListVM"/></returns>
        public async Task<JsonResult> OnPostImageAsync(IList<IFormFile> images)
        {
            var userId = Convert.ToInt32(_userManager.GetUserId(HttpContext.User));
            List<string> urls = new List<string>();

            foreach (var image in images)
            {
                using (Stream stream = image.OpenReadStream())
                {
                    var media = await _blogSvc.UploadImageAsync(stream, userId, image.FileName, image.ContentType, EUploadedFrom.Browser);
                    urls.Add(_blogSvc.GetImageUrl(media, EImageSize.Small));
                }
            }

            // TODO
            var (medias, count) = await GetImageVMsAsync(1);
            return new JsonResult(medias);
        }

        /// <summary>
        /// DELETE an image by id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<JsonResult> OnDeleteAsync(int id)
        {
            await _blogSvc.DeleteImageAsync(id);

            // refresh
            var (medias, count) = await GetImageVMsAsync(1);
            return new JsonResult(medias);
        }

        public async Task<JsonResult> OnPostUpdateAsync([FromBody]ImageVM media)
        {
            await _mediaSvc.UpdateMediaAsync(media.Id, media.Title, media.Caption, media.Alt, media.Description);
            // TODO
            var (medias, count) = await GetImageVMsAsync(1);
            return new JsonResult(medias);
        }

        // -------------------------------------------------------------------- private

        /// <summary>
        /// Returns 
        /// </summary>
        /// <remarks>
        /// TODO check each media AppType to decide which GetImageUrl to call
        /// </remarks>
        private async Task<(IEnumerable<ImageVM> medias, int count)> GetImageVMsAsync(int pageNumber)
        {
            var (medias, count) = await _mediaSvc.GetMediasAsync(EMediaType.Image, pageNumber, PAGE_SIZE);
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var appName = EAppType.Blog.ToString().ToLowerInvariant();
           
            var imageListVm = from m in medias
                              select new ImageVM
                              {
                                  Id = m.Id,
                                  FileName = m.FileName,
                                  Title = m.Title,
                                  Caption = m.Caption,
                                  Alt = m.Alt,
                                  FileType = m.ContentType,
                                  UploadDate = m.UploadedOn.ToString("yyyy-MM-dd"),
                                  UploadVia = m.UploadedFrom.ToString(),
                                  Width = m.Width,
                                  Height = m.Height,
                                  UrlSmall = _blogSvc.GetImageUrl(m, EImageSize.Small),
                                  UrlMedium = _blogSvc.GetImageUrl(m, EImageSize.Medium),
                                  UrlLarge = _blogSvc.GetImageUrl(m, EImageSize.Large), 
                                  UrlOriginal = _blogSvc.GetImageUrl(m, EImageSize.Original), 
                              };

            return (medias: imageListVm, count: count);
        }
    }
}
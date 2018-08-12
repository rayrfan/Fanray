using Fan.Blog.Enums;
using Fan.Blog.Services;
using Fan.Medias;
using Fan.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
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

        public class ImageListVM
        {
            public IEnumerable<ImageVM> Images { get; set; }
            public int TotalImages { get; set; }
        }

        public class ImageVM : Media
        {
            public string Url { get; set; }
        }

        public async Task<JsonResult> OnGetImagesAsync()
        {
            var list = await GetImageListVMAsync();
            return new JsonResult(list);
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
            var list = await GetImageListVMAsync();
            return new JsonResult(list);
        }

        // -------------------------------------------------------------------- private

        /// <summary>
        /// This is shared by get and post images, not ideal.
        /// </summary>
        /// <returns></returns>
        private async Task<ImageListVM> GetImageListVMAsync()
        {
            var list = await _mediaSvc.GetMediasAsync(EMediaType.Image, 1, 50);
            var user = await _userManager.GetUserAsync(HttpContext.User);

            var appName = EAppType.Blog.ToString().ToLowerInvariant();

            var imageListVm = from m in list
                              select new ImageVM
                              {
                                  Id = m.Id,
                                  FileName = m.FileName,
                                  Title = m.Title,
                                  Caption = m.Caption,
                                  Alt = m.Alt,
                                  Url = _blogSvc.GetImageUrl(m, EImageSize.Small), // gallery uses small
                              };

            return new ImageListVM
            {
                Images = imageListVm,
            };
        }
    }
}
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
        private readonly IMediaService _mediaSvc;
        private readonly UserManager<User> _userManager;

        public MediaModel(IMediaService mediaSvc,
                          UserManager<User> userManager)
        {
            _mediaSvc = mediaSvc;
            _userManager = userManager;
        }

        public class ImageListVM
        {
            public IEnumerable<ImageVM> Images { get; set; }
            public int TotalImages { get; set; }
        }

        public class ImageVM
        {
            public int Id { get; set; }
            public string FileName { get; set; }
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
        /// </remarks>
        /// <returns><see cref="ImageListVM"/></returns>
        public async Task<JsonResult> OnPostImageAsync(IList<IFormFile> images)
        {
            var userId = Convert.ToInt32(_userManager.GetUserId(HttpContext.User));

            foreach (var image in images)
            {
                using (Stream stream = image.OpenReadStream())
                {
                    await _mediaSvc.UploadImageAsync(stream, EAppType.Blog, userId,
                        image.FileName, EUploadedFrom.Browser);
                }
            }

            var list = await GetImageListVMAsync();
            return new JsonResult(list);
        }

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
                                  Url = m.Optimized ? $"{MediaService.IMAGE_HANDLER_PATH}/{appName}/optimized/{user.Id}/{m.UploadedOn.Year}/{m.UploadedOn.Month.ToString("d2")}/{m.FileName}" :
                                  $"{MediaService.IMAGE_HANDLER_PATH}/{appName}/original/{user.Id}/{m.UploadedOn.Year}/{m.UploadedOn.Month.ToString("d2")}/{m.FileName}",
                              };

            return new ImageListVM
            {
                Images = imageListVm,
            };
        }
    }
}
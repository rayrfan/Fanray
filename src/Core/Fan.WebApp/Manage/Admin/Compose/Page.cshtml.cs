using Fan.Blog.Enums;
using Fan.Blog.Helpers;
using Fan.Blog.Models;
using Fan.Blog.Models.Input;
using Fan.Blog.Models.View;
using Fan.Blog.Services.Interfaces;
using Fan.Exceptions;
using Fan.Helpers;
using Fan.Membership;
using Fan.Settings;
using Markdig;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fan.WebApp.Manage.Admin.Compose
{
    /// <summary>
    /// Page composer.
    /// </summary>
    public class PageModel : Microsoft.AspNetCore.Mvc.RazorPages.PageModel
    {
        private const string DATE_FORMAT = "yyyy-MM-dd";
        private readonly UserManager<User> userManager;
        private readonly IPageService pageService;
        private readonly ISettingService settingService;

        public const int AUTOSAVE_INTERVAL = 10;
        public string PageJson { get; set; }

        public PageModel(UserManager<User> userManager,
                         IPageService pageService,
                         ISettingService settingService)
        {
            this.userManager = userManager;
            this.pageService = pageService;
            this.settingService = settingService;
        }

        /// <summary>
        /// Prepares a <see cref="PageIM"/> for composer to work with.
        /// </summary>
        /// <param name="pageId"></param>
        /// <param name="parentId"></param>
        /// <returns></returns>
        /// <remarks>
        /// When <paramref name="pageId"/> is present (greater than 0), the user is updating an exisiting page,
        /// in this situation the <paramref name="parentId"/> is ignored as it is not part of the update.
        /// </remarks>
        public async Task OnGetAsync(int pageId, int? parentId)
        {
            var coreSettings = await settingService.GetSettingsAsync<CoreSettings>();
            Blog.Models.Page parent = null;
            PageIM pageIM;

            if (pageId > 0) // edit a page, ignore parentId
            {
                var page = await pageService.GetAsync(pageId);
                if (page.ParentId.HasValue && page.ParentId > 0)
                {
                    parent = await pageService.GetAsync(page.ParentId.Value);
                }

                pageIM = new PageIM
                {
                    Id = page.Id,
                    BodyMark = page.BodyMark,
                    DraftDate = page.UpdatedOn.HasValue ? page.UpdatedOn.Value.ToString(DATE_FORMAT) : "",
                    IsDraft = page.Status == EPostStatus.Draft,
                    IsParentDraft = parent != null ? parent.Status == EPostStatus.Draft : false,
                    ParentId = page.ParentId,
                    PostDate = page.CreatedOn.ToString(DATE_FORMAT),
                    Published = page.Status == EPostStatus.Published,
                    Title = page.Title,
                };
            }
            else // new post
            {
                if (parentId.HasValue && parentId > 0)
                {
                    parent = await pageService.GetAsync(parentId.Value);
                }

                var date = Util.ConvertTime(DateTimeOffset.UtcNow, coreSettings.TimeZoneId).ToString(DATE_FORMAT);
                pageIM = new PageIM
                {
                    Title = "",
                    BodyMark = "",
                    PostDate = date,
                    ParentId = parentId,
                    Published = false,
                    IsDraft = false,
                    IsParentDraft = parent != null ? parent.Status == EPostStatus.Draft : false,
                };
            }
            PageJson = JsonConvert.SerializeObject(pageIM);
        }

        /// <summary>
        /// Publishes a post.
        /// </summary>
        /// <returns>
        /// Absolute URL to the post.
        /// </returns>
        /// <remarks>
        /// The post could be new or previously published.
        /// </remarks>
        public async Task<IActionResult> OnPostPublishAsync([FromBody]PageIM pageIM)
        {
            try
            {
                var page = new Blog.Models.Page
                {
                    UserId = Convert.ToInt32(userManager.GetUserId(HttpContext.User)),
                    ParentId = pageIM.ParentId,
                    CreatedOn = BlogUtil.GetCreatedOn(pageIM.PostDate),
                    Title = pageIM.Title,
                    BodyMark = pageIM.BodyMark,
                    Status = EPostStatus.Published,
                };

                if (pageIM.Id <= 0)
                {
                    page = await pageService.CreateAsync(page);
                }
                else
                {
                    page.Id = pageIM.Id;
                    page = await pageService.UpdateAsync(page);
                }

                return new JsonResult(GetPostAbsoluteUrl(page));
            }
            catch (FanException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Updates an existing published post.
        /// </summary>
        /// <returns>
        /// Absolute URL to the post.
        /// </returns>
        public async Task<IActionResult> OnPostUpdateAsync([FromBody]PageIM pageIM)
        {
            try
            {
                var page = new Blog.Models.Page
                {
                    Id = pageIM.Id,
                    UserId = Convert.ToInt32(userManager.GetUserId(HttpContext.User)),
                    ParentId = pageIM.ParentId,
                    CreatedOn = BlogUtil.GetCreatedOn(pageIM.PostDate),
                    Title = pageIM.Title,
                    BodyMark = pageIM.BodyMark,
                    Status = EPostStatus.Published,
                };

                page = await pageService.UpdateAsync(page);
                return new JsonResult(GetPostAbsoluteUrl(page));
            }
            catch (FanException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Saves a page as draft.
        /// </summary>
        /// <returns>
        /// The updated <see cref="BlogPost"/>.
        /// </returns>
        /// <remarks>
        /// This is called by either Auto Save or user clicking on Save.
        /// </remarks>
        public async Task<IActionResult> OnPostSaveAsync([FromBody]PageIM pageIM)
        {
            try
            {
                var page = new Blog.Models.Page
                {
                    UserId = Convert.ToInt32(userManager.GetUserId(HttpContext.User)),
                    ParentId = pageIM.ParentId,
                    CreatedOn = BlogUtil.GetCreatedOn(pageIM.PostDate),
                    Title = pageIM.Title,
                    BodyMark = pageIM.BodyMark,
                    Status = EPostStatus.Draft,
                };

                if (pageIM.Id <= 0)
                {
                    page = await pageService.CreateAsync(page);
                }
                else
                {
                    page.Id = pageIM.Id;
                    page = await pageService.UpdateAsync(page);
                }

                pageIM = new PageIM
                {
                    Id = page.Id,
                    Title = page.Title,
                    BodyMark = page.BodyMark,
                    PostDate = page.CreatedOn.ToString(DATE_FORMAT),
                    ParentId = page.ParentId,
                    Published = page.Status == EPostStatus.Published,
                    IsDraft = page.Status == EPostStatus.Draft,
                    DraftDate = page.UpdatedOn.HasValue ? page.UpdatedOnDisplay : "",
                };

                return new JsonResult(pageIM);
            }
            catch (FanException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Returns preview url.
        /// </summary>
        /// <param name="pageIM"></param>
        /// <returns></returns>
        public async Task<JsonResult> OnPostPreviewAsync([FromBody]PageIM pageIM)
        {
            // title
            var title = pageIM.Title.IsNullOrEmpty() ? "Untitled" : pageIM.Title;

            // slug
            var slug = BlogUtil.SlugifyPageTitle(pageIM.Title);
            if (slug.IsNullOrEmpty()) slug = "untitled";

            // parent slug
            var parentSlug = "";
            if (pageIM.ParentId.HasValue && pageIM.ParentId > 0)
            {
                var parent = await pageService.GetAsync(pageIM.ParentId.Value);
                parentSlug = parent.Slug;
            }

            // body 
            var body = Markdown.ToHtml(pageIM.BodyMark);

            // author
            var user = await userManager.GetUserAsync(HttpContext.User);
            var author = user.DisplayName;

            // date
            var coreSettings = await settingService.GetSettingsAsync<CoreSettings>();
            var date = DateTimeOffset.Parse(pageIM.PostDate).ToDisplayString(coreSettings.TimeZoneId, "dddd, MMMM dd, yyyy");

            // TempData
            var prevRelLink = parentSlug.IsNullOrEmpty() ? BlogRoutes.GetPagePreviewRelativeLink(slug) :
                              BlogRoutes.GetPagePreviewRelativeLink(parentSlug, slug);
            var pageVM = new PageVM
            {
                Author = author,
                Body = body,
                CreatedOnDisplay = date,
                Slug = $"{parentSlug}/{slug}",
                Title = title,
            };
            TempData.Put(prevRelLink, pageVM);

            // return preview url
            return new JsonResult($"{Request.Scheme}://{Request.Host}{prevRelLink}");
        }

        private string GetPostAbsoluteUrl(Blog.Models.Page page)
        {
            var relativeUrl = page.IsParent ?
                BlogRoutes.GetPageRelativeLink(page.Slug) :
                BlogRoutes.GetPageRelativeLink(page.Parent.Slug, page.Slug);

            return $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}{relativeUrl}";
        }
    }
}
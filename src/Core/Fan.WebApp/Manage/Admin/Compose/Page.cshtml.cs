using Fan.Blog.Enums;
using Fan.Blog.Helpers;
using Fan.Blog.Models;
using Fan.Blog.Models.Input;
using Fan.Blog.Models.View;
using Fan.Blog.Services;
using Fan.Blog.Services.Interfaces;
using Fan.Exceptions;
using Fan.Membership;
using Fan.Settings;
using Fan.Themes;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly IThemeService themeService;
        private readonly ISettingService settingService;

        public const int AUTOSAVE_INTERVAL = 10;
        public string PageJson { get; set; }
        public string LayoutsJson { get; set; }

        public PageModel(UserManager<User> userManager,
                         IPageService pageService,
                         IThemeService themeService,
                         ISettingService settingService)
        {
            this.userManager = userManager;
            this.pageService = pageService;
            this.themeService = themeService;
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
        public async Task<IActionResult> OnGetAsync(int pageId, int? parentId)
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

                // convert utc to user local time
                var postDate = page.CreatedOn.ToLocalTime(coreSettings.TimeZoneId).ToString(DATE_FORMAT);
                pageIM = new PageIM
                {
                    Id = page.Id,
                    BodyMark = page.BodyMark,
                    DraftDate = page.UpdatedOn.HasValue ? page.UpdatedOn.Value.ToString(DATE_FORMAT) : "",
                    Excerpt = page.Excerpt,
                    IsDraft = page.Status == EPostStatus.Draft,
                    IsParentDraft = parent != null ? parent.Status == EPostStatus.Draft : false,
                    ParentId = page.ParentId,
                    PostDate = postDate,
                    Published = page.Status == EPostStatus.Published,
                    Title = page.Title,
                    PageLayout = (EPageLayout) page.PageLayout,
                };
            }
            else // new post
            {
                if (parentId.HasValue && parentId > 0)
                {
                    parent = await pageService.GetAsync(parentId.Value);
                    if (!parent.IsParent) return Redirect("/admin/pages"); // make sure parent is really parent
                }

                // convert utc to user local time
                var date = DateTimeOffset.UtcNow.ToLocalTime(coreSettings.TimeZoneId).ToString(DATE_FORMAT);
                pageIM = new PageIM
                {
                    Title = "",
                    BodyMark = "",
                    PostDate = date,
                    ParentId = parentId,
                    Published = false,
                    IsDraft = false,
                    IsParentDraft = parent != null ? parent.Status == EPostStatus.Draft : false,
                    PageLayout = parent != null ? (EPageLayout) parent.PageLayout : EPageLayout.Layout1,
                };
            }
            PageJson = JsonConvert.SerializeObject(pageIM);

            // layouts
            var currentTheme = (await themeService.GetManifestsAsync())
                   .Single(t => t.Name.Equals(coreSettings.Theme, StringComparison.OrdinalIgnoreCase));
            LayoutsJson = JsonConvert.SerializeObject(currentTheme.PageLayouts);

            return Page();
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
                    Body = pageIM.Body,
                    BodyMark = pageIM.BodyMark,
                    Excerpt = pageIM.Excerpt,
                    Status = EPostStatus.Published,
                    PageLayout = (byte) pageIM.PageLayout,
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
                    Body = pageIM.Body,
                    BodyMark = pageIM.BodyMark,
                    Excerpt = pageIM.Excerpt,
                    Status = EPostStatus.Published,
                    PageLayout = (byte) pageIM.PageLayout,
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
                // get page
                var page = new Blog.Models.Page
                {
                    UserId = Convert.ToInt32(userManager.GetUserId(HttpContext.User)),
                    ParentId = pageIM.ParentId,
                    CreatedOn = BlogUtil.GetCreatedOn(pageIM.PostDate),
                    Title = pageIM.Title,
                    Body = pageIM.Body,
                    BodyMark = pageIM.BodyMark,
                    Excerpt = pageIM.Excerpt,
                    Status = EPostStatus.Draft,
                    PageLayout = (byte) pageIM.PageLayout,
                };

                // create or update page
                if (pageIM.Id <= 0)
                {
                    page = await pageService.CreateAsync(page);
                }
                else
                {
                    page.Id = pageIM.Id;
                    page = await pageService.UpdateAsync(page);
                }

                // return page
                var coreSettings = await settingService.GetSettingsAsync<CoreSettings>();
                pageIM = new PageIM
                {
                    Id = page.Id,
                    Title = page.Title,
                    BodyMark = page.BodyMark,
                    Excerpt = page.Excerpt,
                    PostDate = page.CreatedOn.ToString(DATE_FORMAT),
                    ParentId = page.ParentId,
                    Published = page.Status == EPostStatus.Published,
                    IsDraft = page.Status == EPostStatus.Draft,
                    DraftDate = page.UpdatedOn.HasValue ? page.UpdatedOn.Value.ToDisplayString(coreSettings.TimeZoneId) : "",
                    PageLayout = (EPageLayout) page.PageLayout,
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

            // body 
            var body = pageIM.Body;

            // author
            var user = await userManager.GetUserAsync(HttpContext.User);
            var author = user.DisplayName;

            // date
            var coreSettings = await settingService.GetSettingsAsync<CoreSettings>();
            var date = DateTimeOffset.Parse(pageIM.PostDate).ToDisplayString(coreSettings.TimeZoneId);

            // layout
            var pageLayout = pageIM.PageLayout;

            // slug
            var slug = PageService.SlugifyPageTitle(pageIM.Title);
            if (slug.IsNullOrEmpty()) slug = "untitled";

            // parent slug
            var parentSlug = "";
            if (pageIM.ParentId.HasValue && pageIM.ParentId > 0)
            {
                var parent = await pageService.GetAsync(pageIM.ParentId.Value);
                parentSlug = parent.Slug;
            }

            // preview relative link (the slugs are url encoded)
            var prevRelLink = parentSlug.IsNullOrEmpty() ? BlogRoutes.GetPagePreviewRelativeLink(slug) :
                              BlogRoutes.GetPagePreviewRelativeLink(parentSlug, slug);

            // put vm in tempdata with preview link as key
            var pageVM = new PageVM
            {
                Author = author,
                Body = body,                
                CreatedOnDisplay = date,
                Slug = $"{parentSlug}/{slug}",
                Title = title,
                PageLayout = pageLayout,
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
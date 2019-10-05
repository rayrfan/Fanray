using Fan.Blog.Enums;
using Fan.Blog.Helpers;
using Fan.Blog.Services.Interfaces;
using Fan.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fan.WebApp.Manage.Admin
{
    public class PagesModel : PageModel
    {
        private readonly IPageService pageService;
        private readonly ISettingService settingService;

        public PagesModel(IPageService pageService,
            ISettingService settingService)
        {
            this.pageService = pageService;
            this.settingService = settingService;
        }

        public string PagesJson { get; set; }
        public int ParentId { get; set; }

        public const string POST_DATE_STRING_FORMAT = "yyyy-MM-dd";

        /// <summary>
        /// Displays either a list of parents or a parent with its child pages.
        /// </summary>
        /// <param name="parentId"></param>
        /// <returns></returns>
        public async Task OnGetAsync(int parentId)
        {
            var pageVMs = await GetPageVMsAsync(parentId);
            PagesJson = JsonConvert.SerializeObject(pageVMs);
            ParentId = parentId;
        }

        public async Task<IActionResult> OnDeleteAsync(int pageId)
        {
            try
            {
                await pageService.DeleteAsync(pageId);
                return new EmptyResult();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Returns a list of <see cref="PageAdminVM"/>, when <paramref name="parentId"/> is present 
        /// the returned list contains the parent and its children, otherwise a list of parents are
        /// returned.
        /// </summary>
        /// <param name="parentId"></param>
        /// <remarks>
        /// When a page or its parent is draft, its PageLink is null.
        /// </remarks>
        private async Task<List<PageAdminVM>> GetPageVMsAsync(int parentId)
        {
            var pageVMs = new List<PageAdminVM>();
            IList<Blog.Models.Page> pages;
            bool isChild = false;
            Fan.Blog.Models.Page parent = null;
            var coreSettings = await settingService.GetSettingsAsync<CoreSettings>();

            if (parentId <= 0)
            {
                pages = await pageService.GetParentsAsync(withChildren: true);
            }
            else
            {
                parent = await pageService.GetAsync(parentId);
                pages = parent.Children;
                isChild = true;

                pageVMs.Add(new PageAdminVM
                {
                    Id = parent.Id,
                    Title = parent.Title,
                    Date = parent.CreatedOn.ToLocalTime(coreSettings.TimeZoneId).ToString(POST_DATE_STRING_FORMAT),
                    Author = parent.User.DisplayName,
                    EditLink = BlogRoutes.GetPageEditLink(parent.Id),
                    IsDraft = parent.Status == EPostStatus.Draft,
                    PageLink = parent.Status == EPostStatus.Draft ? null :
                            $"{Request.Scheme}://{Request.Host}" + BlogRoutes.GetPageRelativeLink(parent.Slug),
                    ChildCount = parent.Children.Count,
                    ViewCount = parent.ViewCount,
                });
            }

            foreach (var page in pages)
            {
                var pageLink = parent != null && parent.Status == EPostStatus.Published?
                        $"{Request.Scheme}://{Request.Host}" + BlogRoutes.GetPageRelativeLink(parent.Slug, page.Slug) :
                        $"{Request.Scheme}://{Request.Host}" + BlogRoutes.GetPageRelativeLink(page.Slug);

                pageLink = page.Status == EPostStatus.Draft || (parent != null && parent.Status == EPostStatus.Draft) ? null : pageLink;

                pageVMs.Add(new PageAdminVM
                {
                    Id = page.Id,
                    Title = page.Title,
                    Date = page.CreatedOn.ToLocalTime(coreSettings.TimeZoneId).ToString(POST_DATE_STRING_FORMAT),
                    Author = page.User.DisplayName,
                    ChildrenLink = !isChild && page.Children.Count > 0 ? $"{Request.Path}/{page.Id}" : "",
                    EditLink = BlogRoutes.GetPageEditLink(page.Id),
                    PageLink = pageLink,
                    IsChild = isChild,
                    IsDraft = page.Status == EPostStatus.Draft,
                    ChildCount = isChild ? 0 : page.Children.Count,
                    ViewCount = page.ViewCount,
                });
            }

            return pageVMs;
        }     
    }

    /// <summary>
    /// Page view model for Admin Console.
    /// </summary>
    public class PageAdminVM
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Date { get; set; }
        public string Author { get; set; }
        public string ChildrenLink { get; set; }
        public string EditLink { get; set; }
        public string PageLink { get; set; }
        public bool IsDraft { get; set; }
        public bool IsChild { get; set; }
        public int ChildCount { get; set; }
        public int ViewCount { get; set; }
    }
}
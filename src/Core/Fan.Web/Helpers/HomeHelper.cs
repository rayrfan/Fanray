using Fan.Blog.Helpers;
using Fan.Blog.Models;
using Fan.Blog.Models.View;
using Fan.Blog.Services;
using Fan.Blog.Services.Interfaces;
using Fan.Settings;
using Fan.Themes;
using Fan.Web.Models.Blog;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fan.Web.Helpers
{
    /// <summary>
    /// Helps prepare the site root content.
    /// </summary>
    /// <remarks>
    /// Ideally after HomeController decides what to render for the site root it should then 
    /// forward the control to the corresponding controller's action to render the content. 
    /// However I didn't figure out how to use methods such as RedirectToAction to do the forward 
    /// while still maintaining the root url "/", this last part "still maintaining the url" 
    /// is key. If anyone knows how to do it, please submit me a PR.
    /// </remarks>
    public class HomeHelper : IHomeHelper
    {
        private readonly IBlogPostService blogPostService;
        private readonly IPageService pageService;
        private readonly ICategoryService categoryService;
        private readonly ISettingService settingService;
        private readonly HttpContext httpContext;

        public HomeHelper(IBlogPostService blogPostService,
            IPageService pageService,
            ICategoryService categoryService,
            ISettingService settingService,
            IHttpContextAccessor httpContextAccessor)
        {
            this.blogPostService = blogPostService;
            this.pageService = pageService;
            this.categoryService = categoryService;
            this.settingService = settingService;
            httpContext = httpContextAccessor.HttpContext;
        }

        public async Task<(string viewPath, BlogPostListViewModel viewModel)> GetBlogIndexAsync(int? page)
        {
            if (!page.HasValue || page <= 0) page = BlogPostService.DEFAULT_PAGE_INDEX;
            var blogSettings = await settingService.GetSettingsAsync<BlogSettings>();
            var posts = await blogPostService.GetListAsync(page.Value, blogSettings.PostPerPage);
            return ("../Blog/Index", new BlogPostListViewModel(posts, blogSettings, httpContext.Request, page.Value));
        }

        public async Task<(string viewPath, BlogPostListViewModel viewModel)> GetBlogCategoryAsync(string slug)
        {
            var cat = await categoryService.GetAsync(slug);
            var posts = await blogPostService.GetListForCategoryAsync(slug, 1);
            var blogSettings = await settingService.GetSettingsAsync<BlogSettings>();
            return ("../Blog/Categories", new BlogPostListViewModel(posts, blogSettings, httpContext.Request, cat));
        }

        public async Task<(string viewPath, PageVM viewModel)> GetPageAsync(string parentPage, string childPage = null)
        {
            if (parentPage.IsNullOrEmpty()) parentPage = "Home";

            var page = await pageService.GetAsync(parentPage, childPage);
            var coreSettings = await settingService.GetSettingsAsync<CoreSettings>();

            return ("../Blog/Page", new PageVM
            {
                Author = page.User.DisplayName,
                Body = page.Body,
                Excerpt = page.Excerpt,
                CreatedOnDisplay = page.CreatedOn.ToDisplayString(coreSettings.TimeZoneId),
                EditLink = BlogRoutes.GetPageEditLink(page.Id),
                IsParent = page.IsParent,
                Title = page.Title,
                PageLayout = (EPageLayout)page.PageLayout,
            });
        }
    }
}

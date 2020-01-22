using Fan.Blog.Helpers;
using Fan.Blog.Models;
using Fan.Blog.Services;
using Fan.Blog.Services.Interfaces;
using Fan.Widgets;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace PageNavigation.Components
{
    /// <summary>
    /// The BlogTags view component.
    /// </summary>
    public class PageNavigationViewComponent : ViewComponent
    {
        private readonly IPageService pageService;
        public PageNavigationViewComponent(IPageService pageService)
        {
            this.pageService = pageService;
        }

        public async Task<IViewComponentResult> InvokeAsync(Widget widget)
        {
            if (widget == null || !Request.Path.HasValue)
            {
                return await Task.FromResult<IViewComponentResult>(Content(string.Empty));
            }

            // slugs
            var slugs = Request.Path.Value.Split('/', StringSplitOptions.RemoveEmptyEntries);

            // if no slugs or slug is reserved, return empty
            if (slugs.Length <= 0 || 
                Array.IndexOf(PageService.Reserved_Slugs, slugs[0].ToLower()) != -1 ||
                Array.IndexOf(PageService.Reserved_Slugs, slugs[^1].ToLower()) != -1)
            {
                return await Task.FromResult<IViewComponentResult>(Content(string.Empty));
            }

            // page
            Page page;
            if (slugs[0] == "preview")
            {
                var composePageId = GetComposeUrlPageId();
                if (composePageId == 0) 
                {
                    return await Task.FromResult<IViewComponentResult>(Content(string.Empty));
                }

                page = await pageService.GetAsync(composePageId);
            }
            else
            {
                page = await pageService.GetAsync(slugs);
            }

            // visible only parent has children
            if (page.IsParent && !page.HasChildren)
            {
                return await Task.FromResult<IViewComponentResult>(Content(string.Empty));
            }

            // nav str
            string navBody;
            int pageId;
            if (page.IsParent)
            {
                navBody = PageService.NavMdToHtml(page.Nav, page.Slug);
                pageId = page.Id;
            }
            else
            {
                var parent = await pageService.GetAsync(page.ParentId.Value);
                navBody = PageService.NavMdToHtml(parent.Nav, parent.Slug);
                pageId = parent.Id;
            }

            // edit link
            var editNavLink = BlogRoutes.GetPageNavEditLink(pageId);

            // wid
            var pageNavigationWidget = (PageNavigationWidget)widget;

            return View("~/Components/PageNavigation.cshtml", 
                new PageNavigationVM { NavBody = navBody, 
                                       NavEditLink = editNavLink,
                                       WidgetTitle = pageNavigationWidget.Title
                });
        }

        /// <summary>
        /// Returns the page id from the "Referer" url which is from the composer.
        /// </summary>
        /// <returns></returns>
        private int GetComposeUrlPageId()
        {
            var url = Request.Headers["Referer"].ToString(); // https://localhost:44381/admin/compose/page/101
            var segs = url.Split('/', StringSplitOptions.RemoveEmptyEntries);
            int.TryParse(segs[^1], out int id);
            return id;
        }
    }

    public class PageNavigationVM
    {
        public string NavBody { get; set; }
        public string NavEditLink { get; set; }
        public string WidgetTitle { get; set; }
    }
}
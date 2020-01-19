using Fan.Blog.Helpers;
using Fan.Blog.Services;
using Fan.Blog.Services.Interfaces;
using Fan.Widgets;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using System.Linq;

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

            // visible only slug is right
            var slugs = Request.Path.Value.Split('/', StringSplitOptions.RemoveEmptyEntries); 

            // when in preview
            if (slugs.Length >= 3)
            {
                if (slugs[0] == "preview") slugs = slugs.Skip(1).ToArray();
                if (slugs[0] == "page") slugs = slugs.Skip(1).ToArray();
            }
                
            if (slugs.Length <= 0 || Array.IndexOf(PageService.Reserved_Slugs, slugs[0].ToLower()) != -1)
            {
                return await Task.FromResult<IViewComponentResult>(Content(string.Empty));
            }

            // visible only parent has children
            var page = await pageService.GetAsync(Request.Path.ToString().StartsWith("/preview"), slugs);
            if (page.IsParent && !page.HasChildren)
            {
                return await Task.FromResult<IViewComponentResult>(Content(string.Empty));
            }

            // nav str
            string nav;
            int pageId;
            if (page.IsParent)
            {
                nav = PageService.NavMdToHtml(page.Nav, page.Slug);
                pageId = page.Id;
            }
            else
            {
                var parent = await pageService.GetAsync(page.ParentId.Value);
                nav = PageService.NavMdToHtml(parent.Nav, parent.Slug);
                pageId = parent.Id;
            }

            // edit link
            var editNavLink = BlogRoutes.GetPageNavEditLink(pageId);

            // wid
            var pageNavigationWidget = (PageNavigationWidget)widget;

            return View("~/Components/PageNavigation.cshtml", new Tuple<string, string, PageNavigationWidget>(nav, editNavLink, pageNavigationWidget));
        }
    }
}
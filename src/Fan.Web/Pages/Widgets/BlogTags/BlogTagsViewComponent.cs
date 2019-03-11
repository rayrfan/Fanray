using Fan.Blog.Models;
using Fan.Blog.Services.Interfaces;
using Fan.Web.Pages.Widgets.BlogTags;
using Fan.Widgets;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fan.Web.Pages.Widgets
{
    /// <summary>
    /// The BlogTags view component.
    /// </summary>
    public class BlogTagsViewComponent : ViewComponent
    {
        private readonly ITagService _tagSvc;
        public BlogTagsViewComponent(ITagService tagService)
        {
            _tagSvc = tagService;
        }

        public async Task<IViewComponentResult> InvokeAsync(Widget widget)
        {
            var blogTagsWidget = (BlogTagsWidget)widget;
            var tags = (await _tagSvc.GetAllAsync()).Where(t => t.Count > 0).Take(blogTagsWidget.MaxTagsDisplayed);

            return View("~/Pages/Widgets/BlogTags/BlogTags.cshtml",
                new Tuple<IEnumerable<Tag>, BlogTagsWidget>(tags, blogTagsWidget));
        }
    }
}
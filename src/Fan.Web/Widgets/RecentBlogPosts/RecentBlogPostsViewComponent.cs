using Fan.Blog.Helpers;
using Fan.Blog.Services.Interfaces;
using Fan.Widgets;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fan.Web.Widgets.RecentBlogPosts
{
    public class RecentBlogPostsViewComponent : ViewComponent
    {
        private readonly IBlogPostService blogPostService;
        private readonly IHttpContextAccessor httpContextAccessor;

        public RecentBlogPostsViewComponent(IBlogPostService blogPostService, 
            IHttpContextAccessor httpContextAccessor)
        {
            this.blogPostService = blogPostService;
            this.httpContextAccessor = httpContextAccessor;
        }

        public async Task<IViewComponentResult> InvokeAsync(Widget widget)
        {
            var recentBlogPostsWidget = (RecentBlogPostsWidget)widget;
            var posts = await blogPostService.GetRecentPostsAsync(recentBlogPostsWidget.NumberOfPostsToShow);

            // must have at least 2 posts 
            if (posts.PostCount < 2)
                return await Task.FromResult<IViewComponentResult>(Content(string.Empty));

            // get current url
            var relativeUrl = httpContextAccessor.HttpContext.Request.Path;
            var list = new List<RecentPostViewModel>();
            foreach (var post in posts.Posts)
            {
                // if post url is current url then skip this post
                var postUrl = BlogRoutes.GetPostRelativeLink(post.CreatedOn, post.Slug);
                if (postUrl.Equals(relativeUrl, StringComparison.OrdinalIgnoreCase)) continue;

                list.Add(new RecentPostViewModel
                {
                    Title = post.Title,
                    Link = BlogRoutes.GetPostPermalink(post.Id),
                    Author = post.User.DisplayName,
                    Excerpt = recentBlogPostsWidget.ShowPostExcerpt ? post.Excerpt : null,
                    Date = recentBlogPostsWidget.ShowPostDate ? post.CreatedOn.ToString("yyyy-MM-dd") : null,
                });
            }

            return View(WidgetService.GetWidgetViewPath("RecentBlogPosts"),
                new Tuple<List<RecentPostViewModel>, RecentBlogPostsWidget>(list, recentBlogPostsWidget));
        }
    }

    public class RecentPostViewModel
    {
        public string Title { get; set; }
        public string Link { get; set; } 
        public string Author { get; set; }
        public string Date { get; set; }
        public string Excerpt { get; set; }
    }
}
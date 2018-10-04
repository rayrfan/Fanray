using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using System;

namespace Fan.Blog.Helpers
{
    public static class BlogRoutes
    {
        private const string POST_RELATIVE_URL_TEMPLATE = "post/{0}/{1}/{2}/{3}";
        private const string POST_PERMA_URL_TEMPLATE = "post/{0}";
        private const string POST_EDIT_URL_TEMPLATE = "admin/compose/{0}";
        private const string CATEGORY_URL_TEMPLATE = "posts/categorized/{0}";
        private const string CATEGORY_RSS_URL_TEMPLATE = "posts/categorized/{0}/feed";
        private const string TAG_URL_TEMPLATE = "posts/tagged/{0}";
        private const string ARCHIVE_URL_TEMPLATE = "posts/{0}/{1}";

        /// <summary>
        /// Returns a blog post's relative link that starts with "/" and contains 2-digit month and day.
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="slug"></param>
        /// <returns></returns>
        public static string GetPostRelativeLink(DateTimeOffset dt, string slug)
        {
            return string.Format("/" + POST_RELATIVE_URL_TEMPLATE, dt.Year, dt.Month.ToString("00"), dt.Day.ToString("00"), slug);
        }

        /// <summary>
        /// Returns a blog post's permalink, the returned string is a relative URL that starts with "/".
        /// </summary>
        /// <param name="postId"></param>
        /// <returns></returns>
        public static string GetPostPermalink(int postId)
        {
            return string.Format("/" + POST_PERMA_URL_TEMPLATE, postId);
        }

        /// <summary>
        /// Returns a blog post's edit link, the returned string is a relative URL that starts with "/".
        /// </summary>
        /// <param name="postId"></param>
        /// <returns></returns>
        public static string GetPostEditLink(int postId)
        {
            return string.Format("/" + POST_EDIT_URL_TEMPLATE, postId);
        }

        /// <summary>
        /// Returns a blog category's relative link that start with "/".
        /// </summary>
        /// <param name="slug"></param>
        /// <returns></returns>
        public static string GetCategoryRelativeLink(string slug)
        {
            return string.Format("/" + CATEGORY_URL_TEMPLATE, slug);
        }

        /// <summary>
        /// Returns a blog category's RSS link, the returned string is relative URL that starts with "/"
        /// </summary>
        /// <param name="slug"></param>
        /// <returns></returns>
        public static string GetCategoryRssRelativeLink(string slug)
        {
            return string.Format("/" + CATEGORY_RSS_URL_TEMPLATE, slug);
        }

        /// <summary>
        /// Returns a blog tag's relative link that start with "/".
        /// </summary>
        /// <param name="slug"></param>
        /// <returns></returns>
        public static string GetTagRelativeLink(string slug)
        {
            return string.Format("/" + TAG_URL_TEMPLATE, slug);
        }

        /// <summary>
        /// Returns a blog archive's relative link that starts with "/" and contains 2-digit month.
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        public static string GetArchiveRelativeLink(int year, int month)
        {
            return string.Format("/" + ARCHIVE_URL_TEMPLATE, year, month.ToString("00"));
        }

        /// <summary>
        /// Registers the blog app's routes.
        /// </summary>
        /// <param name="routes"></param>
        public static void RegisterRoutes(IRouteBuilder routes)
        {
            routes.MapRoute("RSD", "rsd", new { controller = "Blog", action = "Rsd" });

            routes.MapRoute("BlogPostPerma", string.Format(POST_PERMA_URL_TEMPLATE, "{id}"),
               new { controller = "Blog", action = "PostPerma", id = 0 }, new { id = @"^\d+$" });

            routes.MapRoute("BlogPost", string.Format(POST_RELATIVE_URL_TEMPLATE, "{year}", "{month}", "{day}", "{slug}"),
                new { controller = "Blog", action = "Post", year = 0, month = 0, day = 0, slug = "" },
                new { year = @"^\d+$", month = @"^\d+$", day = @"^\d+$" });

            // "posts/categorized/technology" shows posts in technology category
            routes.MapRoute("BlogCategory", string.Format(CATEGORY_URL_TEMPLATE, "{slug}"),
                new { controller = "Blog", action = "Category", slug = "" });

            // "posts/tagged/cs" shows posts with c# tag
            routes.MapRoute("BlogTag", string.Format(TAG_URL_TEMPLATE, "{slug}"),
                new { controller = "Blog", action = "Tag", slug = "" });

            // "posts/2017/12" shows posts from Dec 2017
            routes.MapRoute("BlogArchive", string.Format(ARCHIVE_URL_TEMPLATE, "{year}", "{month}"),
                new { controller = "Blog", action = "Archive", year = 0, month = 0 },
                new { year = @"^\d+$", month = @"^\d+$" });

            // "feed" shows main feed of blog
            routes.MapRoute("BlogFeed", "feed", new { controller = "Blog", action = "Feed" });

            // "posts/categorized/technology/feed"
            routes.MapRoute("BlogCategoryFeed", string.Format(CATEGORY_RSS_URL_TEMPLATE, "{slug}"), 
                new { controller = "Blog", action = "CategoryFeed", slug = "" });
        }
    }
}
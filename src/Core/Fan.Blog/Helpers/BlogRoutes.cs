using Fan.Navigation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using System;

namespace Fan.Blog.Helpers
{
    public static class BlogRoutes
    {
        // URL templates

        private const string PAGE_PARENT_RELATIVE_URL = "{0}";
        private const string PAGE_PARENT_CHILD_RELATIVE_URL = "{0}/{1}";
        private const string PAGE_EDIT_URL = "admin/compose/page/{0}";
        private const string PAGE_EDIT_NAV_URL = "admin/compose/pagenav/{0}";
        private const string PREVIEW_PARENT_RELATIVE_URL = "preview/page/{0}";
        private const string PREVIEW_PARENT_CHILD_RELATIVE_URL = "preview/page/{0}/{1}";

        private const string POST_RELATIVE_URL = "post/{0}/{1}/{2}/{3}";
        private const string PREVIEW_POST_RELATIVE_URL = "preview/post/{0}/{1}/{2}/{3}";
        private const string POST_PERMA_URL = "blog/post/{0}";
        private const string POST_EDIT_URL = "admin/compose/post/{0}";

        private const string CATEGORY_URL = "posts/categorized/{0}";
        private const string CATEGORY_RSS_URL = "posts/categorized/{0}/feed";
        private const string TAG_URL = "posts/tagged/{0}";
        private const string ARCHIVE_URL = "posts/{0}/{1}";

        /// <summary>
        /// Returns a page's relative link that starts with "/" and contains one or two slugs.
        /// </summary>
        /// <param name="slugs"></param>
        /// <returns></returns>
        public static string GetPageRelativeLink(params string[] slugs)
        {
            return slugs.Length <= 1 || slugs[1].IsNullOrEmpty() ?
                string.Format("/" + PAGE_PARENT_RELATIVE_URL, slugs[0]):
                string.Format("/" + PAGE_PARENT_CHILD_RELATIVE_URL, slugs[0], slugs[1]);
        }

        /// <summary>
        /// Returns a page's edit link, the returned string is a relative URL that starts with "/".
        /// </summary>
        /// <param name="pageId"></param>
        /// <returns></returns>
        public static string GetPageEditLink(int pageId)
        {
            return string.Format("/" + PAGE_EDIT_URL, pageId);
        }

        public static string GetPageNavEditLink(int pageId)
        {
            return string.Format("/" + PAGE_EDIT_NAV_URL, pageId);
        }

        /// <summary>
        /// Returns a page's preview relative link that starts with "/".
        /// </summary>
        /// <param name="slugs"></param>
        /// <returns></returns>
        public static string GetPagePreviewRelativeLink(params string[] slugs)
        {
            return slugs.Length <= 1 || slugs[1].IsNullOrEmpty() ?
                string.Format("/" + PREVIEW_PARENT_RELATIVE_URL, slugs[0]) :
                string.Format("/" + PREVIEW_PARENT_CHILD_RELATIVE_URL, slugs[0], slugs[1]);
        }

        /// <summary>
        /// Returns a blog post's relative link that starts with "/" and contains 2-digit month and day.
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="slug"></param>
        /// <returns></returns>
        public static string GetPostRelativeLink(DateTimeOffset dt, string slug)
        {
            return string.Format("/" + POST_RELATIVE_URL, dt.Year, dt.Month.ToString("00"), dt.Day.ToString("00"), slug);
        }

        /// <summary>
        /// Returns a blog post's preview relative link that starts with "/".
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="slug"></param>
        /// <returns></returns>
        public static string GetPostPreviewRelativeLink(DateTimeOffset dt, string slug)
        {
            return string.Format("/" + PREVIEW_POST_RELATIVE_URL, dt.Year, dt.Month.ToString("00"), dt.Day.ToString("00"), slug);
        }

        /// <summary>
        /// Returns a blog post's permalink, the returned string is a relative URL that starts with "/".
        /// </summary>
        /// <param name="postId"></param>
        /// <returns></returns>
        public static string GetPostPermalink(int postId)
        {
            return string.Format("/" + POST_PERMA_URL, postId);
        }

        /// <summary>
        /// Returns a blog post's edit link, the returned string is a relative URL that starts with "/".
        /// </summary>
        /// <param name="postId"></param>
        /// <returns></returns>
        public static string GetPostEditLink(int postId)
        {
            return string.Format("/" + POST_EDIT_URL, postId);
        }

        /// <summary>
        /// Returns a blog category's relative link that start with "/".
        /// </summary>
        /// <param name="slug"></param>
        /// <returns></returns>
        public static string GetCategoryRelativeLink(string slug)
        {
            return string.Format("/" + CATEGORY_URL, slug);
        }

        /// <summary>
        /// Returns a blog category's RSS link, the returned string is relative URL that starts with "/"
        /// </summary>
        /// <param name="slug"></param>
        /// <returns></returns>
        public static string GetCategoryRssRelativeLink(string slug)
        {
            return string.Format("/" + CATEGORY_RSS_URL, slug);
        }

        /// <summary>
        /// Returns a blog tag's relative link that start with "/".
        /// </summary>
        /// <param name="slug"></param>
        /// <returns></returns>
        public static string GetTagRelativeLink(string slug)
        {
            return string.Format("/" + TAG_URL, slug);
        }

        /// <summary>
        /// Returns a blog archive's relative link that starts with "/" and contains 2-digit month.
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        public static string GetArchiveRelativeLink(int year, int month)
        {
            return string.Format("/" + ARCHIVE_URL, year, month.ToString("00"));
        }

        /// <summary>
        /// Registers the blog app's routes.
        /// </summary>
        /// <param name="routes"></param>
        public static void RegisterRoutes(IRouteBuilder routes)
        {
            // "blog"
            routes.MapRoute("Blog", App.BLOG_APP_URL, new { controller = "Blog", action = "Index" });

            // "rsd"
            routes.MapRoute("RSD", "rsd", new { controller = "Blog", action = "Rsd" });

            // "blog/post/1"
            routes.MapRoute("BlogPostPerma", string.Format(POST_PERMA_URL, "{id}"),
               new { controller = "Blog", action = "PostPerma", id = 0 }, new { id = @"^\d+$" });

            // "post/2017/01/01/test-post"
            routes.MapRoute("BlogPost", string.Format(POST_RELATIVE_URL, "{year}", "{month}", "{day}", "{slug}"),
                new { controller = "Blog", action = "Post", year = 0, month = 0, day = 0, slug = "" },
                new { year = @"^\d+$", month = @"^\d+$", day = @"^\d+$" });

            // "preview/post/2017/01/01/test-post"
            routes.MapRoute("BlogPreview", string.Format(PREVIEW_POST_RELATIVE_URL, "{year}", "{month}", "{day}", "{slug}"),
                new { controller = "Blog", action = "PreviewPost", year = 0, month = 0, day = 0, slug = "" },
                new { year = @"^\d+$", month = @"^\d+$", day = @"^\d+$" });

            // "posts/categorized/technology"
            routes.MapRoute("BlogCategory", string.Format(CATEGORY_URL, "{slug}"),
                new { controller = "Blog", action = "Category", slug = "" });

            // "posts/tagged/cs" 
            routes.MapRoute("BlogTag", string.Format(TAG_URL, "{slug}"),
                new { controller = "Blog", action = "Tag", slug = "" });

            // "posts/2017/12" 
            routes.MapRoute("BlogArchive", string.Format(ARCHIVE_URL, "{year}", "{month}"),
                new { controller = "Blog", action = "Archive", year = 0, month = 0 },
                new { year = @"^\d+$", month = @"^\d+$" });

            // "feed"
            routes.MapRoute("BlogFeed", "feed", new { controller = "Blog", action = "Feed" });

            // "posts/categorized/technology/feed"
            routes.MapRoute("BlogCategoryFeed", string.Format(CATEGORY_RSS_URL, "{slug}"),
                new { controller = "Blog", action = "CategoryFeed", slug = "" });

            // "preview/page/about"
            routes.MapRoute("PagePreview", string.Format(PREVIEW_PARENT_RELATIVE_URL, "{parentSlug}"),
                new { controller = "Blog", action = "PreviewPage", parentSlug = "" });

            // "preview/page/about/ray"
            routes.MapRoute("ChildPagePreview", string.Format(PREVIEW_PARENT_CHILD_RELATIVE_URL, "{parentSlug}", "{childSlug}"),
                new { controller = "Blog", action = "PreviewPage", parentSlug = "", childSlug = "" });

            // "about"
            routes.MapRoute("Page", "{parentPage}",
                defaults: new { controller = "Blog", action = "Page", parentPage = "" });

            // "about/ray"
            routes.MapRoute("ChildPage", "{parentPage}/{childPage}",
                defaults: new { controller = "Blog", action = "Page", parentPage = "", childPage = "" });
        }
    }
}
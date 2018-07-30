using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Fan.Blog.Helpers
{
    public static class BlogRoutes
    {
        public const string POST_RELATIVE_URL_TEMPLATE = "post/{0}/{1}/{2}/{3}";
        public const string POST_PERMA_URL_TEMPLATE = "post/{0}";
        public const string POST_EDIT_URL_TEMPLATE = "admin/compose/{0}";
        public const string CATEGORY_URL_TEMPLATE = "posts/categorized/{0}";
        public const string CATEGORY_RSS_URL_TEMPLATE = "posts/categorized/{0}/feed";
        public const string TAG_URL_TEMPLATE = "posts/tagged/{0}";
        public const string ARCHIVE_URL_TEMPLATE = "posts/{0}/{1}";

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

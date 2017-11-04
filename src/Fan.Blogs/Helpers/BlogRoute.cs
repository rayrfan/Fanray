using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Fan.Blogs.Helpers
{
    public static class BlogRoute
    {
        public static void RegisterRoutes(IRouteBuilder routes)
        {
            routes.MapRoute("RSD", "rsd", new { controller = "Blog", action = "Rsd" });

            routes.MapRoute("BlogPostPerma", string.Format(BlogConst.POST_PERMA_URL_TEMPLATE, "{id}"),
               new { controller = "Blog", action = "PostPerma", id = 0 }, new { id = @"^\d+$" });

            routes.MapRoute("BlogPost", string.Format(BlogConst.POST_RELATIVE_URL_TEMPLATE, "{year}", "{month}", "{day}", "{slug}"),
                new { controller = "Blog", action = "Post", year = 0, month = 0, day = 0, slug = "" },
                new { year = @"^\d+$", month = @"^\d+$", day = @"^\d+$" });

            routes.MapRoute("BlogCategory", string.Format(BlogConst.CATEGORY_URL_TEMPLATE, "{slug}"),
                new { controller = "Blog", action = "Category", slug = "" });

            routes.MapRoute("BlogTag", string.Format(BlogConst.TAG_URL_TEMPLATE, "{slug}"),
                new { controller = "Blog", action = "Tag", slug = "" });

            routes.MapRoute("BlogFeed", "feed", new { controller = "Blog", action = "Feed" });
            routes.MapRoute("BlogCategoryFeed", string.Format(BlogConst.CATEGORY_RSS_URL_TEMPLATE, "{slug}"), 
                new { controller = "Blog", action = "CategoryFeed", slug = "" });
        }
    }
}

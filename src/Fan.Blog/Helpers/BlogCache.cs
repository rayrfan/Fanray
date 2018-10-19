using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Threading.Tasks;

namespace Fan.Blog.Helpers
{
    /// <summary>
    /// Blog cache keys and intervals and helpers.
    /// </summary>
    public class BlogCache
    {
        public const string KEY_POSTS_INDEX = "BlogPostsIndex";
        public const string KEY_ALL_CATS = "BlogCategories";
        public const string KEY_ALL_TAGS = "BlogTags";
        public const string KEY_ALL_ARCHIVES = "BlogArchives";
        public const string KEY_POST_COUNT = "BlogPostCount";

        /// <summary>
        /// 10 minutes.
        /// </summary>
        public static readonly TimeSpan Time_PostsIndex = new TimeSpan(0, 10, 0);
        /// <summary>
        /// 10 minutes.
        /// </summary>
        public static readonly TimeSpan Time_AllCats = new TimeSpan(0, 10, 0);
        /// <summary>
        /// 10 minutes.
        /// </summary>
        public static readonly TimeSpan Time_AllTags = new TimeSpan(0, 10, 0);
        /// <summary>
        /// 10 minutes.
        /// </summary>
        public static readonly TimeSpan Time_Archives = new TimeSpan(0, 10, 0);
        /// <summary>
        /// 10 minutes.
        /// </summary>
        public static readonly TimeSpan Time_PostCount = new TimeSpan(0, 10, 0);

        /// <summary>
        /// Remove all cached objects for blog.
        /// </summary>
        /// <param name="cache"></param>
        /// <returns></returns>
        public static async Task RemoveAllBlogCacheAsync(IDistributedCache cache)
        {
            await cache.RemoveAsync(KEY_POSTS_INDEX);
            await cache.RemoveAsync(KEY_ALL_CATS);
            await cache.RemoveAsync(KEY_ALL_TAGS);
            await cache.RemoveAsync(KEY_ALL_ARCHIVES);
            await cache.RemoveAsync(KEY_POST_COUNT);
        }
    }
}

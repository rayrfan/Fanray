using System;

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
    }
}

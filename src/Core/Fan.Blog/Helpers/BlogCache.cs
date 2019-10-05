using System;

namespace Fan.Blog.Helpers
{
    /// <summary>
    /// Blog cache keys and intervals.
    /// </summary>
    public class BlogCache
    {
        public const string KEY_POSTS_INDEX = "BlogPostsIndex";
        public const string KEY_POST = "BlogPost_{0}_{1}_{2}_{3}";
        public const string KEY_POST_VIEW_COUNT = "BlogPost_{0}_ViewCount";
        public const string KEY_POST_COUNT = "BlogPostCount";

        public const string KEY_ALL_CATS = "BlogCategories";
        public const string KEY_ALL_TAGS = "BlogTags";
        public const string KEY_ALL_ARCHIVES = "BlogArchives";
        public const string KEY_MAIN_RSSFEED = "BlogRssFeed";
        public const string KEY_CAT_RSSFEED = "BlogRssFeed_{0}";

        public const string KEY_PAGE = "Page_{0}"; 
        public const string KEY_PAGE_VIEW_COUNT = "Page_{0}_ViewCount";

        /// <summary>
        /// 10 minutes.
        /// </summary>
        public static readonly TimeSpan Time_ParentPage = new TimeSpan(0, 10, 0);
        /// <summary>
        /// 2 minutes.
        /// </summary>
        public static readonly TimeSpan Time_ChildPage = new TimeSpan(0, 2, 0);
        /// <summary>
        /// 10 minutes.
        /// </summary>
        public static readonly TimeSpan Time_PostsIndex = new TimeSpan(0, 10, 0);
        /// <summary>
        /// 10 min.
        /// </summary>
        public static readonly TimeSpan Time_SingplePost = new TimeSpan(0, 10, 0);
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
        /// 10 minutes.
        /// </summary>
        public static readonly TimeSpan Time_MainRSSFeed = new TimeSpan(0, 10, 0);
        /// <summary>
        /// 10 minutes.
        /// </summary>
        public static readonly TimeSpan Time_CatRSSFeed = new TimeSpan(0, 10, 0);
        /// <summary>
        /// 10 min.
        /// </summary>
        public static readonly TimeSpan Time_ViewCount = new TimeSpan(0, 10, 0);
    }
}

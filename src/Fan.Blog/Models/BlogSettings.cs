using Fan.Blog.Enums;
using Fan.Settings;

namespace Fan.Blog.Models
{
    /// <summary>
    /// Settings for the blog application.
    /// </summary>
    /// <remarks>
    /// This is saved in the meta table, it's ok to add or remove properties here.
    /// </remarks>
    public class BlogSettings : ISettings
    {
        /// <summary>
        /// Number of blog posts to show on a page. Default 10.
        /// </summary>
        public int PostPerPage { get; set; } = 10; 
        /// <summary>
        /// There must be one default category. Default 1.
        /// </summary>
        public int DefaultCategoryId { get; set; } = 1;
        /// <summary>
        /// What type of display for each blog post in a list of posts.
        /// </summary>
        public EPostListDisplay PostListDisplay { get; set; } = EPostListDisplay.FullBody;

        // -------------------------------------------------------------------- Comments

        /// <summary>
        /// Whether to allow people to post comments on blog posts.
        /// </summary>
        /// <remarks>
        /// When turning on this setting, it will not hide any existing comments, it will not allow
        /// visitors to post comments on new or existing posts.
        /// 
        /// This setting may be overridden by individual posts <see cref="Post.CommentStatus"/>.
        /// </remarks>
        public bool AllowComments { get; set; } = true;
        /// <summary>
        /// Which comment system to use. Default Fanray.
        /// </summary>
        public ECommentProvider CommentProvider { get; set; } = ECommentProvider.Fanray;
        /// <summary>
        /// Disqus shortname. Default is null.
        /// </summary>
        /// <remarks>
        /// https://help.disqus.com/customer/portal/articles/466208-what-s-a-shortname-
        /// </remarks>
        public string DisqusShortname { get; set; }

        // -------------------------------------------------------------------- RSS

        /// <summary>
        /// Gets or sets whether feed should show full text or excerpt. Default false.
        /// </summary>
        /// <remarks>
        /// For each article in a feed, show
        /// </remarks>
        public bool FeedShowExcerpt { get; set; } = false;
    }
}

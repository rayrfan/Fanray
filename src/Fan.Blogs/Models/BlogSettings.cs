using Fan.Blogs.Enums;

namespace Fan.Blogs.Models
{
    /// <summary>
    /// Settings for the blog application.
    /// </summary>
    /// <remarks>
    /// This is saved in the meta table, it's ok to add or remove properties here.
    /// </remarks>
    public class BlogSettings
    {
        /// <summary>
        /// Number of blog posts to show. Default 10.
        /// </summary>
        public int PageSize { get; set; } = 10; 
        /// <summary>
        /// There must be one default category. Default 1.
        /// </summary>
        public int DefaultCategoryId { get; set; } = 1;
        /// <summary>
        /// How many words to extract into excerpt from body. Default 55.
        /// </summary>
        public int ExcerptWordLimit { get; set; } = 55;
        /// <summary>
        /// Should blog show a list of excerpt instead of body. Default false.
        /// </summary>
        public bool ShowExcerpt { get; set; } = false;

        // -------------------------------------------------------------------- Comments

        /// <summary>
        /// Whether to allow people to post comments on blog posts.
        /// </summary>
        /// <remarks>
        /// This is the default value to blog post comment, it can be overriden by individaul post
        /// <see cref="Post.CommentStatus"/>.
        /// </remarks>
        public bool AllowCommentsOnBlogPost { get; set; } = true;

        /// <summary>
        /// Which comment system to use. Default Disqus.
        /// </summary>
        public ECommentProvider CommentProvider { get; set; } = ECommentProvider.Disqus;
        /// <summary>
        /// Disqus shortname. Default is null.
        /// </summary>
        /// <remarks>
        /// https://help.disqus.com/customer/portal/articles/466208-what-s-a-shortname-
        /// </remarks>
        public string DisqusShortname { get; set; }

        // -------------------------------------------------------------------- RSS

        /// <summary>
        /// Gets or sets whether rss feed should show full text or excerpt. Default false.
        /// </summary>
        /// <remarks>
        /// For each article in a feed, show
        /// </remarks>
        public bool RssShowExcerpt { get; set; } = false;
    }
}

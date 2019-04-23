namespace Fan.Blog.Enums
{
    /// <summary>
    /// What type of display for each blog post in a list of posts like shown on blog index page, category or tag pages.
    /// </summary>
    public enum EPostListDisplay
    {
        /// <summary>
        /// Displays post excerpt.
        /// </summary>
        /// <remarks>
        /// <see cref="Post.Excerpt"/> is either manually inputted by user or extracted programatically from post body.
        /// </remarks>
        Excerpt = 0,
        /// <summary>
        /// Displays full post body.
        /// </summary>
        FullBody = 1,
        /// <summary>
        /// Displays post title only.
        /// </summary>
        TitleOnly = 2,
    }
}

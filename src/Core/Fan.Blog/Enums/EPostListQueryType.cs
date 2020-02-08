namespace Fan.Blog.Enums
{
    public enum EPostListQueryType
    {
        /// <summary>
        /// A list of blog posts.
        /// </summary>
        BlogPosts,
        /// <summary>
        /// All blog drafts.
        /// </summary>
        BlogDrafts,
        /// <summary>
        /// Posts for a category
        /// </summary>
        BlogPostsByCategory,
        /// <summary>
        /// Posts for a tag
        /// </summary>
        BlogPostsByTag,
        /// <summary>
        /// Posts for archive.
        /// </summary>
        BlogPostsArchive,
        /// <summary>
        /// A specified number of blog posts regardless status, int.MaxValue will return all.
        /// This is used by metaweblog.
        /// </summary>
        BlogPostsByNumber,
        /// <summary>
        /// A sepcific number of published blog posts.
        /// </summary>
        BlogPublishedPostsByNumber,
        /// <summary>
        /// All parent pages regardless status.
        /// </summary>
        Pages,
        /// <summary>
        /// All pages (parents and their children).
        /// </summary>
        PagesWithChildren,
    }
}
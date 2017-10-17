namespace Fan.Blogs.Enums
{
    public enum EPostListQueryType
    {
        /// <summary>
        /// A list of blog posts.
        /// </summary>
        BlogPosts = 0,
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
        /// A specified number of blog posts regardless status, int.MaxValue will return all.
        /// This is used by metaweblog.
        /// </summary>
        BlogPostsByNumber,

        /// <summary>
        /// All root pages.
        /// </summary>
        RootPages,
        /// <summary>
        /// All child pages for a root page.
        /// </summary>
        ChildPagesForRoot,
        ChildPagesForParent,
        /// <summary>
        /// All draft pages.
        /// </summary>
        PageDrafts,
        /// <summary>
        /// Pages, used by metaweblog.
        /// </summary>
        PagesByNumber,
    }
}
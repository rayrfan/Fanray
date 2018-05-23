using Fan.Blogs.Enums;
using Fan.Blogs.Services;
using System;

namespace Fan.Blogs.Models
{
    /// <summary>
    /// Query for a list of posts.
    /// </summary>
    public class PostListQuery
    {
        /// <summary>
        /// Constructor on <see cref="EPostListQueryType"/> and optional rootId.
        /// </summary>
        /// <exception cref="FanException">
        /// If query is for child pages, then rootId must be a non zero value.
        /// https://msdn.microsoft.com/en-us/library/ms229060(v=vs.110).aspx
        /// </exception>
        public PostListQuery(EPostListQueryType queryType, int? id = null)
        {
            if (queryType == EPostListQueryType.ChildPagesForRoot)
                RootId = id;
            else if (queryType == EPostListQueryType.ChildPagesForParent)
                ParentId = id;
            QueryType = queryType;

            if ((id == null || id <= 0) && queryType == EPostListQueryType.ChildPagesForRoot)
            {
                throw new ArgumentException($"Invalid id '{id}'. A query for child pages must have a valid root id.");
            }
            else if ((id == null || id <= 0) && queryType == EPostListQueryType.ChildPagesForParent)
            {
                throw new ArgumentException($"Invalid id '{id}'. A query for child pages must have a valid parent id.");
            }
        }

        public EPostListQueryType QueryType { get; set; } = EPostListQueryType.BlogPosts;
        public int? RootId { get; set; }
        public int? ParentId { get; set; }
        public int PageIndex { get; set; } = BlogService.DEFAULT_PAGE_INDEX;
        public int PageSize { get; set; } = BlogService.DEFAULT_PAGE_SIZE;
        public string CategorySlug { get; set; }
        public string TagSlug { get; set; }
        public int Year { get; set; }
        public int? Month { get; set; }
    }
}
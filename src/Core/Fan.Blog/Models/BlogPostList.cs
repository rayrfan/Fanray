using System.Collections.Generic;

namespace Fan.Blog.Models
{
    /// <summary>
    /// Results of a <see cref="PostListQuery"/> with a list of blog posts and total number of posts.
    /// </summary>
    public class BlogPostList
    {
        public BlogPostList()
        {
            Posts = new List<BlogPost>();
        }

        /// <summary>
        /// Posts returned by <see cref="PostListQuery"/>
        /// </summary>
        public List<BlogPost> Posts { get; set; }

        /// <summary>
        /// Total number of posts returned for a <see cref="PostListQuery"/>
        /// </summary>
        public int TotalPostCount { get; set; }
    }
}

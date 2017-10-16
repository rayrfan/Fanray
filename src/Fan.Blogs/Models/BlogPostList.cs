using System;
using System.Collections.Generic;

namespace Fan.Blogs.Models
{
    /// <summary>
    /// A list of blog posts.
    /// </summary>
    public class BlogPostList : List<BlogPost>
    {
        /// <summary>
        /// Important to keep a param-less constructor or json serialization will fail for cache.
        /// </summary>
        public BlogPostList()
        {

        }

        public BlogPostList(int postCount, int pageSize)
        {
            PostCount = postCount;
            PageCount = (int)Math.Ceiling(PostCount / (double)pageSize);
        }

        /// <summary>
        /// Total number of posts returned for a <see cref="PostListQuery"/>
        /// </summary>
        public int PostCount { get; set; }
        /// <summary>
        /// Total number of pages based on <see cref="PostCount"/>.
        /// </summary>
        public int PageCount { get; private set; }
    }
}

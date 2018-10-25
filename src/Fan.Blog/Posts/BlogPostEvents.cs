using Fan.Blog.Models;
using MediatR;
using System.Collections.Generic;

namespace Fan.Blog.Posts
{
    /// <summary>
    /// Raised before a blog post is created.
    /// </summary>
    public class BlogPostBeforeCreate : INotification
    {
        /// <summary>
        /// Income category title.
        /// </summary>
        public string CategoryTitle { get; set; }
        /// <summary>
        /// Incoming tag titles.
        /// </summary>
        public List<string> TagTitles { get; set; }
    }

    /// <summary>
    /// Raised before a blog post is updated.
    /// </summary>
    public class BlogPostBeforeUpdate : INotification
    {
        /// <summary>
        /// Incoming category title.
        /// </summary>
        public string CategoryTitle { get; set; }
        /// <summary>
        /// Incoming tag titles.
        /// </summary>
        public List<string> TagTitles { get; set; }
        /// <summary>
        /// The current post.
        /// </summary>
        public Post CurrentPost { get; set; }
    }

    /// <summary>
    /// Raised after a blog post is created.
    /// </summary>
    public class BlogPostCreated : INotification
    {
        public BlogPost BlogPost { get; set; }
    }

    /// <summary>
    /// Raised after a blog post is updated.
    /// </summary>
    public class BlogPostUpdated : INotification
    {
        public BlogPost BlogPost { get; set; }
    }
}

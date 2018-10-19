using Fan.Blog.Models;
using MediatR;
using System.Collections.Generic;

namespace Fan.Blog.Posts
{
    /// <summary>
    /// Called before a blog post is created.
    /// </summary>
    public class BlogPostBeforeCreate : INotification
    {
        public string CategoryTitle { get; set; }
        /// <summary>
        /// Incoming tag titles.
        /// </summary>
        public List<string> TagTitles { get; set; }
    }

    /// <summary>
    /// Called before a blog post is updated.
    /// </summary>
    public class BlogPostBeforeUpdate : INotification
    {
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
    /// Called after a blog post is created.
    /// </summary>
    public class BlogPostCreated : INotification
    {
        public BlogPost BlogPost { get; set; }
    }

    /// <summary>
    /// Called after a blog post is updated.
    /// </summary>
    public class BlogPostUpdated : INotification
    {
        public BlogPost BlogPost { get; set; }
    }
}

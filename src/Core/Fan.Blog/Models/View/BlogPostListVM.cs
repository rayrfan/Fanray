using Fan.Blog.Enums;
using System.Collections.Generic;

namespace Fan.Blog.Models.View
{
    /// <summary>
    /// View model for a list of blog posts.
    /// </summary>
    public class BlogPostListVM
    {
        public BlogPostListVM()
        {
            BlogPostViewModels = new List<BlogPostVM>();
        }

        public List<BlogPostVM> BlogPostViewModels { get; set; }

        public EPostListDisplay PostListDisplay { get; set; }

        /// <summary>
        /// Total number of posts returned for a <see cref="PostListQuery"/>
        /// </summary>
        public int PostCount { get; set; }

        /// <summary>
        /// Tag title to show on Tag.cshtml page.
        /// </summary>
        public string TagTitle { get; set; }
        /// <summary>
        /// Category title to show on Category.cshtml page.
        /// </summary>
        public string CategoryTitle { get; set; }
        /// <summary>
        /// Category or tag description to show on Category.cshtml page.
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Archive title to show on Archive.cshtml page.
        /// </summary>
        public string ArchiveTitle { get; set; }

        public bool ShowOlder { get; set; }
        public bool ShowNewer { get; set; }
        public string OlderPostsUrl { get; set; }
        public string NewerPostsUrl { get; set; }
    }
}

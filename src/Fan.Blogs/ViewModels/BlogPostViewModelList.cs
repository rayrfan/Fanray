using Fan.Blogs.Models;
using System.Collections.Generic;

namespace Fan.Blogs.ViewModels
{
    public class BlogPostViewModelList
    {
        public BlogPostViewModelList()
        {
            PostViewModels = new List<BlogPostViewModel>();
        }

        public List<BlogPostViewModel> PostViewModels { get; set; }
        public BlogSettings Settings { get; set; }
        /// <summary>
        /// Total number of posts returned for a <see cref="PostListQuery"/>
        /// </summary>
        public int PostCount { get; set; }
        /// <summary>
        /// Total number of pages based on <see cref="PostCount"/>.
        /// </summary>
        public int PageCount { get; set; }
    }
}

using Fan.Blogs.Models;
using Fan.Blogs.Services;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Fan.Blogs.ViewModels
{
    /// <summary>
    /// View model for a list of blog posts.
    /// </summary>
    /// <remarks>
    /// This is used on Index.cshtml, Category.cshtml and Tag.cshtml.
    /// </remarks>
    public class BlogPostListViewModel
    {
        public BlogPostListViewModel(BlogPostList blogPostList, BlogSettings blogSettings, HttpRequest request, int currentPage = 1)
        {
            BlogPostViewModels = new List<BlogPostViewModel>();
            foreach (var blogPost in blogPostList.Posts)
            {
                BlogPostViewModels.Add(new BlogPostViewModel(blogPost, blogSettings, request));
            }
            ShowExcerpt = blogSettings.ShowExcerpt;
            PostCount = blogPostList.PostCount;

            if (currentPage <= 0) currentPage = 1;
            if ((currentPage * BlogService.DEFAULT_PAGE_SIZE) < PostCount)
            {
                ShowOlder = true;
                OlderPageIndex = currentPage + 1;
            }
            if (currentPage > 1)
            {
                ShowNewer = true;
                NewerPageIndex = currentPage - 1;
            }
        }

        public BlogPostListViewModel(BlogPostList blogPostList, BlogSettings blogSettings, HttpRequest request, Category cat)
            : this(blogPostList, blogSettings, request)
        {
            CategoryTitle = cat.Title;
        }

        public BlogPostListViewModel(BlogPostList blogPostList, BlogSettings blogSettings, HttpRequest request, Tag tag)
           : this(blogPostList, blogSettings, request)
        {
            TagTitle = tag.Title;
        }

        /// <summary>
        /// The list of <see cref="BlogPostViewModel"/> to show on the page.
        /// </summary>
        public List<BlogPostViewModel> BlogPostViewModels { get; }

        /// <summary>
        /// Whether to show full body or excerpt.
        /// </summary>
        public bool ShowExcerpt { get; }

        /// <summary>
        /// Total number of posts returned for a <see cref="PostListQuery"/>
        /// </summary>
        public int PostCount { get; }

        /// <summary>
        /// Tag title to show on Tag.cshtml page.
        /// </summary>
        public string TagTitle { get; }
        /// <summary>
        /// Category title to show on Category.cshtml page.
        /// </summary>
        public string CategoryTitle { get; }
        /// <summary>
        /// Archive title to show on Archive.cshtml page.
        /// </summary>
        public string ArchiveTitle { get; set; }

        public bool ShowOlder { get; set; }
        public bool ShowNewer { get; set; }
        public int OlderPageIndex { get; set; }
        public int NewerPageIndex { get; set; }
    }
}

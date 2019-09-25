using Fan.Blog.Enums;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Fan.Blog.Models.View
{
    /// <summary>
    /// View model for a list of blog posts.
    /// </summary>
    public class BlogPostListVM
    {
        public BlogPostListVM(BlogPostList blogPostList, BlogSettings blogSettings, HttpRequest request, int currentPage = 1)
        {
            BlogPostViewModels = new List<BlogPostVM>();
            foreach (var blogPost in blogPostList.Posts)
            {
                BlogPostViewModels.Add(new BlogPostVM(blogPost, blogSettings, request));
            }
            PostListDisplay = blogSettings.PostListDisplay;
            PostCount = blogPostList.PostCount;

            if (currentPage <= 0) currentPage = 1;
            if ((currentPage * blogSettings.PostPerPage) < PostCount)
            {
                ShowOlder = true;
                OlderPostsUrl = $"{request.Path}?page={currentPage + 1}";
            }
            if (currentPage > 1)
            {
                ShowNewer = true;
                NewerPostsUrl = currentPage <= 2 ? $"{request.Path}" : $"{request.Path}?page={currentPage - 1}";
            }
        }

        public BlogPostListVM(BlogPostList blogPostList, BlogSettings blogSettings, HttpRequest request, Category cat, int currentPage = 1)
            : this(blogPostList, blogSettings, request, currentPage)
        {
            CategoryTitle = cat.Title;
            Description = cat.Description;
        }

        public BlogPostListVM(BlogPostList blogPostList, BlogSettings blogSettings, HttpRequest request, Tag tag)
           : this(blogPostList, blogSettings, request)
        {
            TagTitle = tag.Title;
            Description = tag.Description;
        }

        /// <summary>
        /// The list of <see cref="BlogPostViewModel"/> to show on the page.
        /// </summary>
        public List<BlogPostVM> BlogPostViewModels { get; }

        /// <summary>
        /// What type of display for each blog post in a list of posts.
        /// </summary>
        public EPostListDisplay PostListDisplay { get; }

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
        /// Category or tag description to show on Category.cshtml page.
        /// </summary>
        public string Description { get; }
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

using Fan.Blogs.Enums;
using Fan.Blogs.Helpers;
using Fan.Blogs.Models;
using Fan.Enums;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace Fan.Blogs.ViewModels
{
    /// <summary>
    /// View model for a blog post.
    /// </summary>
    /// <remarks>
    /// This is used on Index.cshtml, Post.cshtml, _PostInfo.cshtml and _Comments.cshtml views,
    /// as well as <see cref="BlogController.Feed"/>.
    /// </remarks>
    public class BlogPostViewModel
    {
        public BlogPostViewModel(BlogPost blogPost, BlogSettings blogSettings, HttpRequest request)
        {
            Title = blogPost.Title;
            Body = blogPost.Body;
            Excerpt = blogPost.Excerpt;
            Author = blogPost.User.DisplayName;
            CreatedOn = blogPost.CreatedOn;
            CreatedOnFriendly = blogPost.CreatedOnFriendly;
            Tags = blogPost.Tags;
            Category = blogPost.Category;

            RelativeLink = string.Format("/" + BlogConst.POST_RELATIVE_URL_TEMPLATE, CreatedOn.Year, CreatedOn.Month, CreatedOn.Day, blogPost.Slug);
            var permalinkPart = string.Format(BlogConst.POST_PERMA_URL_TEMPLATE, blogPost.Id);
            Permalink = $"{request.Scheme}://{request.Host}/{permalinkPart}";
            CanonicalUrl = $"{request.Scheme}://{request.Host}{RelativeLink}";
            EditLink = string.Format("/" + BlogConst.POST_EDIT_URL_TEMPLATE, blogPost.Id);

            DisqusPageIdentifier = $"{ECommentTargetType.BlogPost}_{blogPost.Id}";
            ShowDisqus = blogSettings.AllowCommentsOnBlogPost && blogSettings.CommentProvider == ECommentProvider.Disqus;
            DisqusShortname = blogSettings.DisqusShortname;
        }

        // -------------------------------------------------------------------- BlogPost

        /// <summary>
        /// The title of the blog post.
        /// </summary>
        public string Title { get; }
        public string Body { get; }
        public string Excerpt { get; }
        /// <summary>
        /// Display name of the user who authored the post.
        /// </summary>
        public string Author { get; }
        /// <summary>
        /// CreatedOn used by blog feeds.
        /// </summary>
        public DateTimeOffset CreatedOn { get; }
        /// <summary>
        /// CreatedOnFriendly used by displaying blog post.
        /// </summary>
        public string CreatedOnFriendly { get; }
        public List<Tag> Tags { get; }
        public Category Category { get; }

        // -------------------------------------------------------------------- Links

        /// <summary>
        /// The relative url of a blog post, used by Index, _PostInfo, Category, Tag views.
        /// </summary>
        public string RelativeLink { get; }
        /// <summary>
        /// The permanent url that redirects to canonical url.
        /// </summary>
        /// <remarks>
        /// This is for tools like disqus or feed readers to have an url that never changes.
        /// </remarks>
        public string Permalink { get; }
        /// <summary>
        /// The url you want search engines to see.
        /// </summary>
        /// <remarks>
        /// The canonical url can change as user can update post slug or post url format,
        /// but it's recommended not to change a post url after publish, if have to it's
        /// preferred to change it asap.
        /// </remarks>
        public string CanonicalUrl { get; }
        /// <summary>
        /// The edit link of a blog post.
        /// </summary>
        public string EditLink { get; }

        // -------------------------------------------------------------------- Comments

        /// <summary>
        /// Returns true if blog comments are enabled in blog settings and comment provider is diqus.
        /// </summary>
        public bool ShowDisqus { get; }

        /// <summary>
        /// The disqus shortname from blog settings.
        /// </summary>
        public string DisqusShortname { get; }

        /// <summary>
        /// A unique string in the format of "{ECommentType.BlogPost}_{post.Id}", this is 
        /// used for both "page.identifier" in _Comments.cshtml and "data-disqus-identifier"
        /// in _PostInfo.cshtml.
        /// </summary>
        public string DisqusPageIdentifier { get; }
    }
}

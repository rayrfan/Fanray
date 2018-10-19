using Fan.Blog.Enums;
using Fan.Blog.Helpers;
using Fan.Blog.Models;
using Fan.Blog.Tags;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Fan.Blog.ViewModels
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
            CreatedOnDisplay = blogPost.CreatedOnDisplay;
            Tags = blogPost.Tags.OrderBy(t => t.Title).ToList();
            Category = blogPost.Category;

            RelativeLink = BlogRoutes.GetPostRelativeLink(CreatedOn, blogPost.Slug);
            var permalinkPart = BlogRoutes.GetPostPermalink(blogPost.Id);
            Permalink = $"{request.Scheme}://{request.Host}{permalinkPart}";
            CanonicalUrl = $"{request.Scheme}://{request.Host}{RelativeLink}";
            EditLink = BlogRoutes.GetPostEditLink(blogPost.Id);

            DisqusPageIdentifier = $"{ECommentTargetType.BlogPost}_{blogPost.Id}";
            ShowDisqus = blogSettings.AllowComments && blogSettings.CommentProvider == ECommentProvider.Disqus && !blogSettings.DisqusShortname.IsNullOrEmpty();
            DisqusShortname = blogSettings.DisqusShortname;

            var hash = "";
            if (blogPost.Tags.Count > 0)
            {
                var sb = new StringBuilder();
                for (int i = 0; i < blogPost.Tags.Count; i++)
                {
                    var tag = blogPost.Tags[i];
                    sb.Append(tag.Slug.Replace("-", ""));
                    if (i<blogPost.Tags.Count-1) sb.Append(",");
                }
                hash = sb.ToString();
            }

            var requestHostShort = request.Host.ToString().StartsWith("www.") ? 
                request.Host.ToString().Remove(0, 4) : request.Host.ToString();
            var permalinkShort = $"{request.Scheme}://{requestHostShort}{permalinkPart}";

            TwitterShareLink = hash.IsNullOrEmpty() ?
                $"https://twitter.com/intent/tweet?text={Title}&url={permalinkShort}" :
                $"https://twitter.com/intent/tweet?text={Title}&url={permalinkShort}&hashtags={hash}";
            FacebookShareLink = $"https://www.facebook.com/sharer/sharer.php?u={permalinkShort}";
            GoogleShareLink = $"https://plus.google.com/share?url={permalinkShort}";
            LinkedInShareLink = $"http://www.linkedin.com/shareArticle?mini=true&url={permalinkShort}&title={Title}";
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
        /// CreatedOn used by displaying blog post.
        /// </summary>
        public string CreatedOnDisplay { get; }
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
        /// Returns true if blog comments are enabled and comment provider is Disqus and DisqusShortname is present.
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

        // -------------------------------------------------------------------- Social links

        public string TwitterShareLink { get; }
        public string FacebookShareLink { get; }
        public string GoogleShareLink { get; }
        public string LinkedInShareLink { get; }
    }
}

using System;
using System.Collections.Generic;

namespace Fan.Blog.Models.View
{
    /// <summary>
    /// View model for a blog post.
    /// </summary>
    public class BlogPostVM
    {
        public string Title { get; set; }
        public string Body { get; set; }
        public string Excerpt { get; set; }
        public string Author { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
        public string CreatedOnDisplay { get; set; }
        public List<Tag> Tags { get; set; }
        public Category Category { get; set; }
        public int ViewCount { get; set; }

        /// <summary>
        /// The relative url of a blog post, used by Index, _PostInfo, Category, Tag views.
        /// </summary>
        public string RelativeLink { get; set; }
        /// <summary>
        /// The permanent url that redirects to canonical url.
        /// </summary>
        /// <remarks>
        /// This is for tools like disqus or feed readers to have an url that never changes.
        /// </remarks>
        public string Permalink { get; set; }
        /// <summary>
        /// The url you want search engines to see.
        /// </summary>
        /// <remarks>
        /// The canonical url can change as user can update post slug or post url format,
        /// but it's recommended not to change a post url after publish, if have to it's
        /// preferred to change it asap.
        /// </remarks>
        public string CanonicalUrl { get; set; }
        /// <summary>
        /// The edit link of a blog post.
        /// </summary>
        public string EditLink { get; set; }

        /// <summary>
        /// Returns true if blog comments are enabled and comment provider is Disqus and DisqusShortname is present.
        /// </summary>
        public bool ShowDisqus { get; set; }

        /// <summary>
        /// The disqus shortname from blog settings.
        /// </summary>
        public string DisqusShortname { get; set; }

        /// <summary>
        /// A unique string in the format of "{ECommentType.BlogPost}_{post.Id}", this is 
        /// used for both "page.identifier" in _Comments.cshtml and "data-disqus-identifier"
        /// in _PostInfo.cshtml.
        /// </summary>
        public string DisqusPageIdentifier { get; set; }

        public string TwitterShareLink { get; set; }
        public string FacebookShareLink { get; set; }
        public string LinkedInShareLink { get; set; }
    }
}

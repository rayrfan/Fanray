using Fan.Blogs.Models;

namespace Fan.Blogs.ViewModels
{
    public class BlogPostViewModel
    {
        public BlogPost BlogPost { get; set; }
        public BlogSettings Settings { get; set; }
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
        /// A unique string in the format of "{ECommentType.BlogPost}_{post.Id}", this is 
        /// used for both "page.identifier" in _Comments.cshtml and "data-disqus-identifier"
        /// in _PostInfo.cshtml.
        /// </summary>
        public string DisqusPageIdentifier { get; set; }
    }
}

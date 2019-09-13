using Fan.Themes;

namespace Fan.Blog.Models.View
{
    /// <summary>
    /// Page view model for client.
    /// </summary>
    public class PageVM
    {
        public string Author { get; set; }
        public string Body { get; set; }
        public string Excerpt { get; set; }
        public string CreatedOnDisplay { get; set;  }
        public string EditLink { get; set; }
        public bool IsParent { get; set; }
        public bool ShowDisqus { get; }
        public string Slug { get; set; }
        public string Title { get; set; }
        public EPageLayout PageLayout { get; set; }
    }
}

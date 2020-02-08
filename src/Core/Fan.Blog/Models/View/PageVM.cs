using Fan.Themes;

namespace Fan.Blog.Models.View
{
    /// <summary>
    /// Page view model for client.
    /// </summary>
    public class PageVM
    {
        public int Id { get; set; }
        public int? ParentId { get; set; }
        public string Author { get; set; }
        public string Body { get; set; }
        public string Excerpt { get; set; }
        public string CreatedOnDisplay { get; set;  }
        public string UpdatedOnDisplay { get; set;  }
        public string EditLink { get; set; }
        public string AddChildLink { get; set; }
        public bool IsParent { get; set; }
        public bool ShowDisqus { get; }
        public string Title { get; set; }
        public int ViewCount { get; set; }
        public EPageLayout PageLayout { get; set; }
    }
}

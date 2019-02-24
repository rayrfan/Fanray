using Fan.Widgets;

namespace Fan.Web.Pages.Widgets.BlogArchives
{
    public class BlogArchivesWidget : Widget
    {
        public BlogArchivesWidget()
        {
            Title = "Archives";
            ShowPostCount = true;
        }

        public bool ShowPostCount { get; set; }
    }
}

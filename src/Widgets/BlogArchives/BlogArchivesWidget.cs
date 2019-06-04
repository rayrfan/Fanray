using Fan.Widgets;

namespace BlogArchives
{
    public class BlogArchivesWidget : Widget
    {
        public BlogArchivesWidget()
        {
            Title = "Archives";
            ShowPostCount = true;
        }

        /// <summary>
        /// Whether to show post count next to archive month.
        /// </summary>
        public bool ShowPostCount { get; set; }
    }
}

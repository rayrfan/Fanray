using Fan.Widgets;

namespace Fan.Web.Pages.Widgets.BlogTags
{
    public class BlogTagsWidget : Widget
    {
        public BlogTagsWidget()
        {
            Title = "Tags";
            MaxTagsDisplayed = 10;
        }

        public int MaxTagsDisplayed { get; set; }
    }
}

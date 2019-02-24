using Fan.Widgets;

namespace Fan.Web.Pages.Widgets.BlogCategories
{
    public class BlogCategoriesWidget : Widget
    {
        public BlogCategoriesWidget()
        {
            Title = "Categories";
            ShowPostCount = true;
        }

        public bool ShowPostCount { get; set; }
    }
}

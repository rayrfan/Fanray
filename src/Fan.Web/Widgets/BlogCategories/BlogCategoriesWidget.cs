using Fan.Widgets;

namespace Fan.Web.Widgets.BlogCategories
{
    public class BlogCategoriesWidget : Widget
    {
        public BlogCategoriesWidget()
        {
            Title = "Categories";
            ShowPostCount = true;
        }

        /// <summary>
        /// Whether to show post count next to category.
        /// </summary>
        public bool ShowPostCount { get; set; }
    }
}

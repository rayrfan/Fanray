using Fan.Widgets;
using System.ComponentModel.DataAnnotations;

namespace Fan.Web.Pages.Widgets.BlogTags
{
    /// <summary>
    /// Blog tags widget. 
    /// </summary>
    /// <remarks>
    /// TODOs 
    /// sort by alph or post count
    /// when not all tags are displayed I should have a link to a tags page to show all
    /// </remarks>
    public class BlogTagsWidget : Widget
    {
        public BlogTagsWidget()
        {
            Title = "Tags";
            MaxTagsDisplayed = 100;
        }

        /// <summary>
        /// Maximum number of tags displayed. Default 100, range must be between 1 and 10,000.
        /// </summary>
        [Range(1, 10000, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public int MaxTagsDisplayed { get; set; }
    }
}

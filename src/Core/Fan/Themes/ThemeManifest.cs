using Fan.Extensibility;
using Fan.Widgets;

namespace Fan.Themes
{
    /// <summary>
    /// The theme.json file.
    /// </summary>
    public class ThemeManifest : Manifest
    {
        /// <summary>
        /// The theme website.
        /// </summary>
        public string ThemeUrl { get; set; }
        /// <summary>
        /// Comma separated strings that describes the theme's characteristics like layout, column,
        /// color, image, text etc. For example, "responsive", "fluid", "fixed", "grid", "one-column", 
        /// "two-column", "three column", "four-column", "right-sidebar", "left-sidebar", 
        /// "featured-images", "rtl-language-support", "sticky-post" etc.
        /// </summary>
        public string[] Tags { get; set; }
        /// <summary>
        /// Widget areas the theme uses.
        /// </summary>
        public WidgetAreaInfo[] WidgetAreas { get; set; }
        /// <summary>
        /// Page layouts.
        /// </summary>
        public PageLayoutInfo[] PageLayouts { get; set; }
    }
}

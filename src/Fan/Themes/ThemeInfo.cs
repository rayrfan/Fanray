using Fan.Extensibility;
using Fan.Widgets;

namespace Fan.Themes
{
    /// <summary>
    /// Information from a theme's theme.json file.
    /// </summary>
    public class ThemeInfo : ManifestInfo
    {
        /// <summary>
        /// URL of the theme.
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
        /// The folder name of the theme, obviously it has to be unique.
        /// </summary>
        /// <remarks>
        /// Theme's folder name could be different from it's Name. 
        /// Not part of theme.json, set during GetInstalledThemesInfoAsync
        /// </remarks>
        public string Folder { get; set; }
        /// <summary>
        /// Widget areas the theme uses.
        /// </summary>
        public WidgetAreaInfo[] WidgetAreas { get; set; }
    }
}

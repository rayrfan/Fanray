using Fan.Widgets;

namespace Fan.Themes
{
    /// <summary>
    /// Information from a theme's theme.json file.
    /// </summary>
    public class ThemeInfo
    {
        /// <summary>
        /// Name of the theme.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Description of the theme.
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Version of the theme.
        /// </summary>
        public string Version { get; set; }
        /// <summary>
        /// URL of the theme.
        /// </summary>
        public string ThemeUrl { get; set; }
        /// <summary>
        /// Name of the author.
        /// </summary>
        public string Author { get; set; }
        /// <summary>
        /// URL of the author.
        /// </summary>
        public string AuthorUrl { get; set; }
        /// <summary>
        /// Theme license.
        /// </summary>
        public string License { get; set; }
        /// <summary>
        /// URL to the license.
        /// </summary>
        public string LicenseUrl { get; set; }
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

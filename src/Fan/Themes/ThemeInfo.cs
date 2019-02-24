namespace Fan.Themes
{
    public class ThemeInfo
    {
        /// <summary>
        /// Display name of the theme.
        /// </summary>
        /// <remarks>
        /// User friendly name to display in Admin Panel.
        /// </remarks>
        public string Name { get; set; }
        public string Version { get; set; }
        public string Author { get; set; }
        public string Url { get; set; }
        public string License { get; set; }
        /// <summary>
        /// "responsive", "fluid", "fixed", "grid".
        /// </summary>
        public string Layout { get; set; }
        /// <summary>
        /// Widget area ids the theme uses.
        /// </summary>
        public string[] WidgetAreas { get; set; }
        /// <summary>
        /// "one-column", "two-column", "three column" "four-column", "right-sidebar", "left-sidebar".
        /// </summary>
        public string Column { get; set; }
        /// <summary>
        /// The folder name of the theme, must be unique.
        /// </summary>
        /// <remarks>
        /// Theme's folder name could be different from it's Name. 
        /// Not part of theme.json, set during GetInstalledThemesInfoAsync
        /// </remarks>
        public string Folder { get; set; }
    }
}

namespace Fan.Widgets
{
    /// <summary>
    /// Information from the "widget.json" file.
    /// </summary>
    public class WidgetInfo
    {
        /// <summary>
        /// Display name.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// the name of folder that contains the widget
        /// </summary>
        //public string Folder { get; set; }
        /// <summary>
        /// short description to show up in Admin Panel
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// The full .NET type "namespace.type, assembly", e.g. "Fan.Web.Widgets.BlogTagsWidget, Fan.Web",
        /// used to instantiate the widget.
        /// </summary>
        public string Type { get; set; }
        public string Version { get; set; }
        public string Author { get; set; }
        public string Url { get; set; }
    }
}

using Fan.Extensibility;

namespace Fan.Widgets
{
    /// <summary>
    /// Information from the "widget.json" file.
    /// </summary>
    public class WidgetInfo : ManifestInfo
    {
        /// <summary>
        /// The full .NET type of the widget "namespace.type, assembly"
        /// used to instantiate the widget.
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// The folder name of the widget, must be unique. It is used as the key to lookup
        /// widget, thus once you decide on a folder name you cannot alter it except only
        /// casing.
        /// </summary>
        /// <remarks>
        /// Widget's folder name could be different from it's Name. 
        /// Not part of widget.json, set during GetInstalledWidgetsInfoAsync.
        /// </remarks>
        public string Folder { get; set; }
    }
}

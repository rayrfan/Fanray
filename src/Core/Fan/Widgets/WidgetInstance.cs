using System;

namespace Fan.Widgets
{
    /// <summary>
    /// A widget instance.
    /// </summary>
    public class WidgetInstance : Widget
    {
        /// <summary>
        /// Display name, <see cref="WidgetManifest.Name"/>.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Url to details page of the widget.
        /// </summary>
        /// <remarks>
        /// TODO info icon on each Available Widgets to show details.
        /// </remarks>
        public new string DetailsUrl => "";

        /// <summary>
        /// Widget settings url.
        /// </summary>
        /// <remarks>
        /// All widgets have settings.
        /// </remarks>
        public new string SettingsUrl =>
            (Folder.IsNullOrEmpty() || Id <= 0) ? "" : $"/{WidgetService.WIDGETS_DIR}/{Folder}Settings?widgetId={Id}";
    }
}

using Fan.Extensibility;
using Newtonsoft.Json;
using System;

namespace Fan.Widgets
{
    /// <summary>
    /// Widget base class.
    /// </summary>
    public class Widget : Extension
    {
        /// <summary>
        /// Id of the widget instance.
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// The id of the area the widget instance resides in.
        /// </summary>
        public string AreaId { get; set; }
        /// <summary>
        /// Widget title (optional). 
        /// </summary>
        /// <remarks>
        /// The title can be left blank and if so the html will not emit for the title.
        /// </remarks>
        public string Title { get; set; }
        /// <summary>
        /// Url to details page of the widget.
        /// </summary>
        /// <remarks>
        /// TODO there should an info icon on each of the Available Widgets, when user clicks on
        /// the icon a widget details page should popup and give detailed info on the widget.
        /// </remarks>
        [JsonIgnore]
        public override string DetailsUrl => "";
        /// <summary>
        /// Url to edit page of the widget.
        /// </summary>
        [JsonIgnore]
        public override string SettingsUrl =>
            (Folder.IsNullOrEmpty() || Id <= 0) ? "" : $"/{WidgetService.WIDGETS_DIR}/{Folder}Edit?widgetId={Id}";
    }
}

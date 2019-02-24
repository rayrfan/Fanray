namespace Fan.Widgets
{
    /// <summary>
    /// A widget instance.
    /// </summary>
    public class WidgetInstance : Widget
    {
        /// <summary>
        /// Id of the widget instance.
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Widget name from widget.json.
        /// </summary>
        public string Name { get; set; }
    }
}

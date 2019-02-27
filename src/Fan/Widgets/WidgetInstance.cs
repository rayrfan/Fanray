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
        /// Display name, <see cref="WidgetInfo.Name"/>.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Folder name, <see cref="WidgetInfo.Folder"/>.
        /// </summary>
        public string Folder { get; set; }
    }
}

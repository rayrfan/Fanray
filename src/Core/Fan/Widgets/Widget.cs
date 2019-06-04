using Fan.Extensibility;

namespace Fan.Widgets
{
    /// <summary>
    /// A widget.
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
    }
}

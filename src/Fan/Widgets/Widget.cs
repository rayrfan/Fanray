using System.ComponentModel.DataAnnotations;

namespace Fan.Widgets
{
    /// <summary>
    /// Widget base class.
    /// </summary>
    public class Widget
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
        /// The full .NET type of the widget "namespace.type, assembly".
        /// </summary>
        public string Type { get; set; }
    }
}

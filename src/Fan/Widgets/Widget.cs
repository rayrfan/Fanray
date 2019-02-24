namespace Fan.Widgets
{
    /// <summary>
    /// Widget base class.
    /// </summary>
    public class Widget
    {
        /// <summary>
        /// Widget title.
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// The full .NET type of the widget "namespace.type, assembly".
        /// </summary>
        public string Type { get; set; }
    }
}

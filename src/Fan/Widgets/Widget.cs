namespace Fan.Widgets
{
    /// <summary>
    /// Widget base class.
    /// </summary>
    public class Widget
    {
        /// <summary>
        /// Id of the widget instance. TODO: should I move this to vm?
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Widget title.
        /// </summary>
        public string Title { get; set; }
    }
}

namespace Fan.Widgets
{
    public class WidgetArea
    {
        public WidgetArea()
        {
            WidgetIds = new int[] { };
        }

        /// <summary>
        /// Pre-defined identifier for the area. e.g. "blog-sidebar1".
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Area title, e.g. "Blog Sidebar1".
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// Widget instances ids the area contains.
        /// </summary>
        public int[] WidgetIds { get; set; }
    }
}

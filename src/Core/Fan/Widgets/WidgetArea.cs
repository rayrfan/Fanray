namespace Fan.Widgets
{
    public class WidgetArea
    {
        public WidgetArea()
        {
            WidgetIds = new int[] { };
        }

        /// <summary>
        /// Id of the area.
        /// </summary>
        /// <remarks>
        /// A theme uses this id to place a widget area on view, 
        /// this info is also in a theme's theme.json file.
        /// </remarks>
        public string Id { get; set; }        
        /// <summary>
        /// Widget instances ids the area contains.
        /// </summary>
        public int[] WidgetIds { get; set; }
    }
}

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
        public string Id { get; set; }        
        /// <summary>
        /// Widget instances ids the area contains.
        /// </summary>
        public int[] WidgetIds { get; set; }
    }
}

using System.Collections.Generic;

namespace Fan.Widgets
{
    /// <summary>
    /// A widget area instance contains a list of <see cref="WidgetInstance"/>.
    /// </summary>
    /// <remarks>
    /// This is an unique challenge in which there is json serialization involved to both database 
    /// and front client. When serializing to database I don't want to include the list of 
    /// widgets but do include them when serializing to front client. JsonIgnore or ShouldSerialize 
    /// would not work in this particular scenario.
    /// </remarks>
    public class WidgetAreaInstance : WidgetArea
    {
        public WidgetAreaInstance()
        {
            WidgetInstances = new List<WidgetInstance>();
            Widgets = new List<Widget>();
        }

        /// <summary>
        /// Widget instances for display.
        /// </summary>
        public List<WidgetInstance> WidgetInstances { get; set; }
        /// <summary>
        /// Widgets for passing into the view components.
        /// </summary>
        public List<Widget> Widgets { get; set; }
    }
}

using System.Collections.Generic;

namespace Fan.Widgets
{
    /// <summary>
    /// A widget area view model with a list of <see cref="Widget"/> in the area.
    /// </summary>
    /// <remarks>
    /// This is an unique challenge in which there is json serialization involved to both database 
    /// and front client. When serializing to database I don't want to include the list of 
    /// widgets but do include them when serializing to front client. JsonIgnore or ShouldSerialize 
    /// would not work in this particular scenario.
    /// </remarks>
    public class WidgetAreaViewModel : WidgetArea
    {
        public WidgetAreaViewModel()
        {
            Widgets = new List<WidgetViewModel>();
        }

        public List<WidgetViewModel> Widgets { get; set; }
    }
}

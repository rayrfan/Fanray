using Fan.Widgets;

namespace Fan.Tests.Widgets
{
    public class MyWidget : Widget
    {
        public int Age { get; set; }
        public string Name { get; set; }

        public MyWidget()
        {
            Title = "My Widget";
            Age = 15;
            Name = "John";
        }
    }
}

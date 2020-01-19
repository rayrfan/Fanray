using Fan.Plugins;

namespace Fan.Tests.Plugins
{
    public class MyPlugin : Plugin
    {
        public int Age { get; set; }
        public string Name { get; set; }

        public MyPlugin()
        {
            Age = 15;
            Name = "Ray";
            Folder = "MyPlugin";
        }
    }
}

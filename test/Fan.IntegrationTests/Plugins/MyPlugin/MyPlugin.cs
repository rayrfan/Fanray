using Fan.Plugins;

namespace Fan.IntegrationTests.Plugins
{
    public class MyPlugin : Plugin
    {
        public int Age { get; set; }
        public string Name { get; set; }

        public MyPlugin()
        {
            Age = 15;
            Name = "John";
        }
    }
}

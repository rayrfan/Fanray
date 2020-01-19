using Fan.Plugins;

namespace Fan.Tests.SysPlugins
{
    public class MySysPlugin : Plugin
    {
        public int Age { get; set; }
        public string Name { get; set; }

        public MySysPlugin()
        {
            Age = 15;
            Name = "Ray";
            Folder = "MySysPlugin";
        }
    }
}

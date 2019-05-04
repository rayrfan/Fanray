using Fan.Extensibility;

namespace Fan.Plugins
{
    /// <summary>
    /// The plugin.json file.
    /// </summary>
    public class PluginManifest : Manifest
    {
        /// <summary>
        /// The plugin meta id.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Returns true if the plugin is active.
        /// </summary>
        public bool Active { get; set; }

        /// <summary>
        /// The plugin website.
        /// </summary>
        public string PluginUrl { get; set; }
    }
}

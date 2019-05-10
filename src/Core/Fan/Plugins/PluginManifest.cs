using Fan.Extensibility;

namespace Fan.Plugins
{
    /// <summary>
    /// The plugin manifest.
    /// </summary>
    /// <remarks>
    /// Contains properties from plugin.json and properties for view.
    /// </remarks>
    public class PluginManifest : Manifest
    {
        /// <summary>
        /// The plugin website.
        /// </summary>
        public string PluginUrl { get; set; }

        /// <summary>
        /// The plugin meta id.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Returns true if the plugin is active.
        /// </summary>
        public bool Active { get; set; }

        /// <summary>
        /// URL to extension settings on Admin Panel.
        /// </summary>
        public string SettingsUrl { get; set; }
    }
}

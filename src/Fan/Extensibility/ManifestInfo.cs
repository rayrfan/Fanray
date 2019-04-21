namespace Fan.Extensibility
{
    /// <summary>
    /// Common properties of theme, plugin and widget json files.
    /// </summary>
    public class ManifestInfo
    {
        /// <summary>
        /// Display name.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Short description to show up in Admin Panel.
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Version.
        /// </summary>
        public string Version { get; set; }
        /// <summary>
        /// Lowest Fanray version it works with.
        /// </summary>
        public string RequiresAtLeast { get; set; }
        /// <summary>
        /// Name of the author.
        /// </summary>
        public string Author { get; set; }
        /// <summary>
        /// URL of the author.
        /// </summary>
        public string AuthorUrl { get; set; }
        /// <summary>
        /// License.
        /// </summary>
        public string License { get; set; }
        /// <summary>
        /// URL to the license.
        /// </summary>
        public string LicenseUrl { get; set; }
    }
}

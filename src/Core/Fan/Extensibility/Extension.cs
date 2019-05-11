using Newtonsoft.Json;

namespace Fan.Extensibility
{
    /// <summary>
    /// The base of all extensible items.
    /// </summary>
    public class Extension
    {
        /// <summary>
        /// Url to extension details page.
        /// </summary>
        /// <remarks>
        /// When this property is null the extension will not display details.
        /// </remarks>
        [JsonIgnore]
        public virtual string DetailsUrl => null;

        /// <summary>
        /// Url to extension settings page.
        /// </summary>
        /// <remarks>
        /// When this property is null the extension will not display settings.
        /// </remarks>
        [JsonIgnore]
        public virtual string SettingsUrl => null;

        /// <summary>
        /// Folder name, <see cref="Manifest.Folder"/>.
        /// </summary>
        public string Folder { get; set; }
    }
}

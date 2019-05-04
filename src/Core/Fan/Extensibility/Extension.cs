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
        [JsonIgnore]
        public virtual string DetailsUrl => null;

        /// <summary>
        /// Url to extension settings page.
        /// </summary>
        [JsonIgnore]
        public virtual string SettingsUrl => null;

        /// <summary>
        /// Folder name, <see cref="Manifest.Folder"/>.
        /// </summary>
        public string Folder { get; set; }
    }
}

using Newtonsoft.Json;

namespace Fan.Navigation
{
    /// <summary>
    /// Represents an item on a navigation.
    /// </summary>
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class Nav
    {
        /// <summary>
        /// Id of the original item.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nav text to display.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Nav link's html title attribute.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The nav type.
        /// </summary>
        public ENavType Type { get; set; }

        /// <summary>
        /// Url of the nav.
        /// </summary>
        /// <remarks>
        /// Only <see cref="ENavType.CustomLink"/> has its url persisted, all the others are 
        /// acquired before sending to the clients.
        /// </remarks>
        public string Url { get; set; }
    }
}

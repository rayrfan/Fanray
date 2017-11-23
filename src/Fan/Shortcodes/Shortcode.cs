using System.Collections.Generic;

namespace Fan.Shortcodes
{
    /// <summary>
    /// The base class all shortcodes inherit from.
    /// </summary>
    public abstract class Shortcode
    {
        /// <summary>
        /// The content between a pair of open and close shortcode tags.
        /// </summary>
        /// <remarks>
        /// If the shortcode has open and close tags, for example,
        /// [code lang=c#] this is the content [/code]
        /// </remarks>
        public string Content { get; set; }
        
        /// <summary>
        /// Shortcode attributes name and value.
        /// </summary>
        public Dictionary<string, object> Attributes { get; set; }

        /// <summary>
        /// Returns the html after processing the shortcode.
        /// </summary>
        /// <returns></returns>
        public abstract string Process();

        /// <summary>
        /// Returns true if the attribute already exists.
        /// </summary>
        /// <param name="attributeName"></param>
        /// <returns></returns>
        protected bool IsSet(string attributeName)
        {
            return Attributes.ContainsKey(attributeName);
        }
    }
}

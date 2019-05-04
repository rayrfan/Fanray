using Fan.Exceptions;
using System;

namespace Fan.Extensibility
{
    /// <summary>
    /// Common properties of an extension's manifest file.
    /// </summary>
    public class Manifest
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
        /// The .NET type string in "namespace.type, assembly" format,
        /// e.g. "BlogTags.BlogTagsWidget, BlogTags". 
        /// </summary>
        /// <remarks>
        /// This property is used to instantiate the plugin/widget/theme.
        /// </remarks>
        public string Type { get; set; }
        /// <summary>
        /// Extension version.
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
        /// Extension license.
        /// </summary>
        public string License { get; set; }
        /// <summary>
        /// URL to the license.
        /// </summary>
        public string LicenseUrl { get; set; }
        /// <summary>
        /// The name of the folder that contains the manifest file.
        /// </summary>
        /// <remarks>
        /// The value must be unique, it is used as the key to lookup an extension. Each extension 
        /// type may have different requirement on folder naming, for example plugin and widget 
        /// requires folder name to be pascal casing.  See implementation of each
        /// <see cref="IExtensibleService{TInfo, TExtension}.IsValidExtensionFolder(string)"/>
        /// method for detail. After you deploy your extension the folder name value cannot altered.
        /// </remarks>
        public string Folder { get; set; }

        /// <summary>
        /// Returns the dll filename based on <see cref="Type"/> property.
        /// </summary>
        /// <returns></returns>
        public string GetDllFileName()
        {
            if (Type.IsNullOrEmpty() || !Type.Contains(','))
                throw new FanException("Invalid \"type\" format in manifest file.");

            var strs = Type.Split(',');
            if (strs.Length != 2)
                throw new FanException("Invalid \"type\" format in manifest file.");

            return $"{strs[1].Trim()}.dll";
        }
    }
}

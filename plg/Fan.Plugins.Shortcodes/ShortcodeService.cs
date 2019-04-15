using Fan.Plugins.Shortcodes.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Fan.Plugins.Shortcodes
{
    /// <summary>
    /// The shortcode service.
    /// </summary>
    /// <remarks>
    /// Based on <see cref="https://github.com/aolde/shortcoder"/>.
    /// </remarks>
    public class ShortcodeService : IShortcodeService
    {
        public ParserState ParserState { get; set; }
        public TextParser TextParser { get; set; }
        public List<ShortcodeParseInfo> ParseInstructions { get; set; }
        public ShortcodeParseInfo CurrentShortcode { get; set; }
        /// <summary>
        /// A dictionary contains tag name (key) and shortcode type (value).
        /// </summary>
        private Dictionary<string, Type> _shortcodes = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Adds shortcode of type T with optional tag name. If tag name is not provided, 
        /// it is deduced from the type name by substracting "shortcode" from the type name.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tag"></param>
        /// <exception cref="InvalidOperationException">
        /// If the shortcode of the provided type has already been added.
        /// </exception>
        public void Add<T>(string tag = null)
        {
            Add(typeof(T), tag);
        }

        /// <summary>
        /// Adds shortcode of type with optional tag name. If tag name is not provided, 
        /// it is deduced from the type name by substracting "shortcode" from the type name.
        /// </summary>
        /// <param name="shortcodeType"></param>
        /// <param name="tag"></param>
        /// <exception cref="InvalidOperationException">
        /// If the shortcode of the provided type has already been added.
        /// </exception>
        public void Add(Type shortcodeType, string tag = null)
        {
            if (tag.IsNullOrEmpty())
            {
                tag = shortcodeType.Name.ToLower().Replace("shortcode", string.Empty);
            }

            if (_shortcodes.ContainsKey(tag))
            {
                throw new InvalidOperationException($"A shortcode named '{tag}' has already been added.");
            }

            _shortcodes.Add(tag, shortcodeType);
        }

        /// <summary>
        /// Creates an instance of the shortcode.
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="attributes"></param>
        /// <param name="content"></param>
        /// <param name="onlyDefined"></param>
        /// <returns></returns>
        public Shortcode Create(string tag, Dictionary<string, object> attributes = null, string content = null)
        {
            var shortcodeType = _shortcodes[tag];
            var shortcode = Activator.CreateInstance(shortcodeType) as Shortcode;

            if (shortcode != null)
            {
                shortcode.Attributes = attributes;
                shortcode.Content = content;

                // convert the Shortcode class property and attribute
                if (shortcode.Attributes != null)
                {
                    var properties = shortcode.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

                    foreach (var attribute in shortcode.Attributes)
                    {
                        var propertyInfo = properties.FirstOrDefault(p => p.Name.Equals(attribute.Key, StringComparison.InvariantCultureIgnoreCase));

                        if (propertyInfo != null)
                        {
                            var type = Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType;
                            var safeValue = attribute.Value == null ? null : Convert.ChangeType(attribute.Value, type);

                            propertyInfo.SetValue(shortcode, safeValue, null);
                        }
                    }
                }
            }

            return shortcode;
        }

        /// <summary>
        /// Returns true if a shortcode with the tag name has been added.
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public bool Exists(string tag)
        {
            return _shortcodes.ContainsKey(tag);
        }

        /// <summary>
        /// Returns a list of shortcodes.
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public List<Shortcode> GetShortcodes(string content)
        {
            ParseInternal(content);

            if (!ParseInstructions.Any())
            {
                return null;
            }

            return ParseInstructions
                .Where(pi => !string.IsNullOrEmpty(pi.Tag))
                .Select(pi => Create(pi.Tag, pi.Attributes, pi.Content))
                .ToList();
        }

        /// <summary>
        /// Parses html body of a post and returns a new html with all shortcodes updated into html.
        /// </summary>
        /// <param name="content">The body of post.</param>
        /// <returns></returns>
        public string Parse(string content)
        {
            ParseInternal(content);

            var contentBuilder = new StringBuilder(content);

            if (ParseInstructions.Any())
            {
                var contentPositionAdjust = 0;
                var initialContentLength = contentBuilder.Length;

                foreach (var shortcodeInfo in ParseInstructions.OrderBy(pi => pi.BeginPosition))
                {
                    string generatedText = null;

                    if (shortcodeInfo.Tag != null)
                    {
                        var shortcode = Create(shortcodeInfo.Tag, shortcodeInfo.Attributes, shortcodeInfo.Content);
                        generatedText = shortcode.Process();
                    }
                    else
                    {
                        generatedText = shortcodeInfo.PreCompiledContent;
                    }

                    if (generatedText == null)
                    {
                        generatedText = string.Empty;
                    }

                    var startIndex = shortcodeInfo.BeginPosition + contentPositionAdjust;
                    if (startIndex < 0)
                    {
                        startIndex = 0;
                    }

                    contentBuilder.Remove(startIndex, shortcodeInfo.EndPosition - shortcodeInfo.BeginPosition);
                    contentBuilder.Insert(startIndex, generatedText);

                    contentPositionAdjust = contentBuilder.Length - initialContentLength;
                }
            }

            return contentBuilder.ToString();
        }

        /// <summary>
        /// Parses the content and initializes the parse instructions.
        /// </summary>
        /// <param name="content"></param>
        private void ParseInternal(string content)
        {
            TextParser = new TextParser(content);
            ParserState = new LookingForTagState(this);
            ParseInstructions = new List<ShortcodeParseInfo>();

            while (!TextParser.IsEndOfText)
            {
                ParserState.Parse();
            }
        }
    }
}

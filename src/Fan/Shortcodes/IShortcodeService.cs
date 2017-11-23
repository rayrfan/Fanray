using System;
using System.Collections.Generic;

namespace Fan.Shortcodes
{
    /// <summary>
    /// The shortcode service.
    /// </summary>
    public interface IShortcodeService
    {
        void Add<T>(string tag = null);
        void Add(Type shortcodeType, string tag = null);
        bool Exists(string tag);
        Shortcode Create(string tag, Dictionary<string, object> attributes = null, string content = null);
        List<Shortcode> GetShortcodes(string content);
        string Parse(string content);
    }
}

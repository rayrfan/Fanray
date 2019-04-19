using System;
using System.Collections.Generic;
using System.Web;

namespace Shortcodes
{
    /// <summary>
    /// YouTube shortcode, current supports only two url formats, https://www.youtube.com/watch?v=MNor4dYXa6U
    /// and https://youtu.be/MNor4dYXa6U while other foramts like "//www.youtube.com/watch?v=MNor4dYXa6U" would fail.
    /// It also supports width, height and start which are attached to the url like 
    /// https://www.youtube.com/watch?v=EHC6sYfF4cg&w=800&h=400&start=75
    /// </summary>
    public class YouTubeShortcode : Shortcode
    {
        public override string Process()
        {
            if (Attributes == null || Attributes.Count <= 0) return "";

            var key = new List<string>(Attributes.Keys)[0];
            var val = (key.Contains("youtu.be/")) ? key.Substring(key.LastIndexOf("youtu.be/") + "youtu.be/".Length) : Attributes[key];
            var url = $"https://www.youtube.com/embed/{val}";

            // default yt size
            var widthSeg = "width=\"560\"";
            var heightSeg = "height=\"315\"";
            if (url.ToLower().Contains("&amp;w=") || url.ToLower().Contains("&amp;start=")) //  height is optional
            {
                url = url.Replace("&amp;", "&");
                string queryStr = new Uri(url).PathAndQuery;
                var dict = HttpUtility.ParseQueryString(queryStr);

                // get all possible query params
                Int32.TryParse(dict["w"], out int width);
                Int32.TryParse(dict["h"], out int height);
                Int32.TryParse(dict["start"], out int start);

                // get rid of query string
                url = url.Substring(0, url.IndexOf("&"));

                widthSeg = width > 0 ? $"width=\"{width}\"" : widthSeg;
                heightSeg = height > 0 ? $"height=\"{height}\"" : heightSeg;

                if (start > 0)
                {
                    url += "?start=" + start;
                }
            }

            return $"<iframe {widthSeg} {heightSeg} src =\"{url}\" frameborder =\"0\" allowfullscreen></iframe>";
        }
    }
}

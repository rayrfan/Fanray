using Fan.Widgets;
using System.Collections.Generic;

namespace Fan.Web.Pages.Widgets.SocialIcons
{
    /// <summary>
    /// Add social-media icons to your site.
    /// </summary>
    public class SocialIconsWidget : Widget
    {
        public SocialIconsWidget()
        {
            Title = "Follow Me";
        }

        /// <summary>
        /// Default is "Follow Me".
        /// </summary>
        //public string Title { get; set; } = "Follow Me";
        //public string Description { get; set; } = "Add social media link to your site.";

        // -------------------------------------------------------------------- widget properties

        /// <summary>
        /// 
        /// </summary>
        public EIconSize Size { get; set; } = EIconSize.Medium;
        /// <summary>
        /// A list of social links.
        /// </summary>
        public IEnumerable<string> Links { get; set; }
        //public IEnumerable<SocialLink> Links { get; set; }

        /// <summary>
        /// https://fontawesome.com/icons?d=gallery&m=free
        /// </summary>
        public static readonly string[] Icons =
        {
            "500px",
            "amazon", "apple",
            "bandcamp", "behance", "bitbucket",
            "codepen",
            "dev", "deviantart", "dribbble", "dropbox",
            "etsy",
            "facebook", "flickr", "foursquare",
            "goodreads", "google", "github", "gitlab", "gitter",
            "instagram", "itunes",
            "link", "linkedin",
            "medium", "meetup",
            "pinterest",
            "reddit", "rss",
            "skype", "slack", "slideshare", "soundcloud", "spotify",
            "tumblr", "twitch", "twitter",
            "vimeo", "vk",
            "weibo",
            "yelp", "youtube",
        };
    }

    public enum EIconSize
    {
        Small,
        Medium,
        Large,
    }
}

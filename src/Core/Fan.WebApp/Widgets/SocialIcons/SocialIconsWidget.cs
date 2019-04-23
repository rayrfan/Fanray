using Fan.Widgets;
using System;
using System.Collections.Generic;

namespace Fan.WebApp.Widgets.SocialIcons
{
    /// <summary>
    /// Add social-media icons to your site.
    /// </summary>
    public class SocialIconsWidget : Widget
    {
        public SocialIconsWidget()
        {
            Title = "Follow Me";
            Links = new List<SocialLink>();
        }

        /// <summary>
        /// A list of social links.
        /// </summary>
        public List<SocialLink> Links { get; set; }

        /// <summary>
        /// Icon match names between user input url domain and icon source 
        /// <see cref="https://github.com/simple-icons/simple-icons"/> 
        /// </summary>
        /// <remarks>
        /// The url domain user inputs must match one of these value for a specific svg icon to show up, 
        /// if the url domain does not match any of the following, it will have the value of "link".
        /// </remarks>
        public static readonly string[] IconNames =
        {
            "500px",
            "amazon", "apple",
            "bandcamp", "behance", "bitbucket",
            "codepen",
            "dev", "deviantart", "dribbble", "dropbox", "discord",
            "etsy",
            "facebook", "flickr", "foursquare",
            "goodreads", "google", "github", "gitlab", "gitter",
            "instagram",
            "linkedin",
            "medium", "meetup",
            "pinterest",
            "reddit", "rss",
            "stackoverflow", "skype", "slack", "slideshare", "soundcloud", "spotify",
            "tumblr", "twitch", "twitter",
            "vimeo", "vk",
            "weibo",
            "yelp", "youtube",
        };

        /// <summary>
        /// Returns my initial social links for seeding.
        /// </summary>
        /// <returns></returns>
        public static List<SocialLink> SocialLinkSeeds = new List<SocialLink>
        {
            new SocialLink { Icon = "rss", Url = "/feed" },
            new SocialLink { Icon = "twitter", Url = "https://twitter.com/fanraymedia" },
            new SocialLink { Icon = "youtube", Url = "https://www.youtube.com/user/fanraymedia" },
            new SocialLink { Icon = "github", Url = "https://github.com/fanraymedia" },
        };

        /// <summary>
        /// Returns a <see cref="SocialLink"/> object given an url. The icon will be "link" icon if
        /// no pre-defined icons match. The icon will be "rss" icon if url ends with "/feed". 
        /// Returns null if url is invalid.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static SocialLink GetSocialLink(string url)
        {
            try
            {
                if (url.EndsWith("/feed")) return new SocialLink { Icon = "rss", Url = url };

                var socialLink = new SocialLink { Icon = "link", Url = url };
                var uri = new Uri(url);
                var host = uri.Host;
                foreach (var icon in IconNames)
                {
                    if (host.Contains(icon, StringComparison.OrdinalIgnoreCase))
                    {
                        socialLink.Icon = icon;
                        break;
                    }
                }

                return socialLink;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }

    public class SocialLink
    {
        public string Icon { get; set; }
        public string Url { get; set; }
    }
}

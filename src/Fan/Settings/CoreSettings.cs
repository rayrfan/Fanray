namespace Fan.Settings
{
    /// <summary>
    /// Core settings for the site.
    /// </summary>
    public class CoreSettings : ISettings
    {
        /// <summary>
        /// Title of the blog. Default "Fanray".
        /// </summary>
        public string Title { get; set; } = "Fanray";
        /// <summary>
        /// Short description on what the blog is about. Default "A fanray blog".
        /// </summary>
        public string Tagline { get; set; } = "A fanray blog";
        /// <summary>
        /// Gets or sets the timezone id for the site. Default "UTC".
        /// </summary>
        /// <remarks>
        /// To learn more about timezone id <see cref="System.TimeZoneInfo.Id"/> and <see cref="http://stackoverflow.com/a/7908482/32240"/>
        /// </remarks>
        public string TimeZoneId { get; set; } = "UTC";
        /// <summary>
        /// The Google Analytics Tracking ID.
        /// </summary>
        /// <remarks>
        /// https://analytics.google.com
        /// </remarks>
        public string GoogleAnalyticsTrackingID { get; set; }

        /// <summary>
        /// Has the setup happened, true will let system skip setup, false will forward to setup page.
        /// </summary>
        public bool SetupDone { get; set; } = false;

        // -------------------------------------------------------------------- Social Links

        /// <summary>
        /// Gets or sets a GitHub page URL.
        /// </summary>
        public string GitHubLink { get; set; }

        /// <summary>
        /// Gets or sets a Facebook page URL.
        /// </summary>
        public string FacebookLink { get; set; }

        /// <summary>
        /// Gets or sets a Twitter URL.
        /// </summary>
        public string TwitterLink { get; set; }

        /// <summary>
        /// Gets or sets a YouTube channel URL.
        /// </summary>
        public string YouTubeLink { get; set; }

        /// <summary>
        /// Gets or sets an Instagram URL.
        /// </summary>
        public string InstagramLink { get; set; }

        /// <summary>
        /// Gets or sets a LinkedIn URL.
        /// </summary>
        public string LinkedInLink { get; set; }
    }
}

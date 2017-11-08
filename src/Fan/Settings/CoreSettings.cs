namespace Fan.Settings
{
    /// <summary>
    /// Core settings.
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
    }
}

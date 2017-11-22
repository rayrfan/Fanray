namespace Fan.Settings
{
    /// <summary>
    /// AppSettings section in appsettings.json.
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// The preferred domain to use: "auto" (default), "www", "nonwww".
        /// </summary>
        /// <remarks>
        /// - "auto" will use whatever the url is given, will not do forward
        /// - "www" will forward root domain to www subdomain, e.g. fanray.com -> www.fanray.com
        /// - "nonwww" will forward www subdomain to root domain, e.g. www.fanray.com -> fanray.com
        /// 
        /// Note if you are running from a subdomian other than "www", preferred domain will be ignored.
        /// This setting is for SEO, it's good to decide on a preferred domain as indicated in this
        /// Google Search Console document https://support.google.com/webmasters/answer/44231?hl=en
        /// </remarks>
        public EPreferredDomain PreferredDomain { get; set; } = EPreferredDomain.Auto;

        /// <summary>
        /// Whether to use https: false (default) or true.
        /// </summary>
        /// <remarks>
        /// - false, will not forward http to https
        /// - true, will forward http to https
        /// 
        /// Note if user sets this value to false but is already using https, I don't downgrade 
        /// you to http as this is good for SEO, Google strongly recommend all website to use https.
        /// Also if you are running locally with console, set this value to false as console may 
        /// not support https.
        /// </remarks>
        public bool UseHttps { get; set; } = false;

        /// <summary>
        /// Whether to use Azure Blob Storage: false (default) will use file system.
        /// </summary>
        public bool UseBlobStorage { get; set; } = false;
    }
}
namespace Fan.Settings
{
    public enum EPreferredDomain
    {
        /// <summary>
        /// use whatever the url is given, will not do forward
        /// </summary>
        Auto,
        /// <summary>
        /// forward root domain to www subdomain, e.g. fanray.com -> www.fanray.com
        /// </summary>
        Www,
        /// <summary>
        /// forward www subdomain to root domain, e.g. www.fanray.com -> fanray.com
        /// </summary>
        NonWww,
    }
}
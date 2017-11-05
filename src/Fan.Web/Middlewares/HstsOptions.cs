using System;

namespace Fan.Web.Middlewares
{
    public class HstsOptions
    {
        /// <summary>
        /// The time that the browser should remember that this site is only to be accessed using HTTPS.
        /// Defaults to 365 days.
        /// </summary>
        public TimeSpan MaxAge { get; set; } = TimeSpan.FromDays(365);

        /// <summary>
        /// Whether this rule applies to all of the site's subdomains as well.
        /// Defaults to <c>true</c>.
        /// </summary>
        public bool IncludeSubdomains { get; set; } = true;

        /// <summary>
        /// See https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Strict-Transport-Security#Preloading_Strict_Transport_Security for details.
        /// Defaults to <c>false</c>.
        /// </summary>
        public bool Preload { get; set; } = false;

        /// <summary>
        /// Whether HSTS headers will be sent when serving to localhost.
        /// Defaults to <c>false</c>;
        /// </summary>
        public bool EnableLocalhost { get; set; } = false;
    }
}

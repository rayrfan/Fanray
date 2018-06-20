using Fan.Settings;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace Fan.Web.Middlewares
{
    public class PreferredDomainRewriter : IPreferredDomainRewriter
    {
        private ILogger<PreferredDomainRewriter> _logger;
        private bool _hostRequireWwwAddition;
        private bool _hostRequireWwwRemoval;

        public PreferredDomainRewriter(ILogger<PreferredDomainRewriter> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Returns true if request requires a url rewrite based on appsettings.
        /// </summary>
        /// <param name="appSettings"></param>
        /// <param name="requestUrl"></param>
        /// <param name="url">The new URL to redirect to as an out parameter.</param>
        /// <returns></returns>
        public bool ShouldRewrite(AppSettings appSettings, string requestUrl, out string url)
        {
            Uri uri = new Uri(requestUrl);
            string host = uri.Authority; // host with port

            // add www if domain does not start with www and domain has only 1 dot, 
            // so yoursite.azurewebsites.net or localhost:1234 would disqualify it
            _hostRequireWwwAddition = appSettings.PreferredDomain == EPreferredDomain.Www &&
                                      !host.StartsWith("www.") &&
                                      host.Count(s => s == '.') == 1;

            // remove www if domain starts with www
            _hostRequireWwwRemoval = appSettings.PreferredDomain == EPreferredDomain.NonWww && host.StartsWith("www.");

            url = GetUrl(uri.Scheme, host, uri.PathAndQuery, uri.Fragment);
            return _hostRequireWwwAddition || _hostRequireWwwRemoval;
        }

        private string GetUrl(string scheme, string host, string pathAndQuery, string fragment)
        {
            if (_hostRequireWwwAddition)
            {
                host = $"www.{host}";
            }
            else if (_hostRequireWwwRemoval)
            {
                int index = host.IndexOf("www.");
                host = host.Remove(index, 4);
            }

            return $"{scheme}://{host}{pathAndQuery}{fragment}";
        }
    }
}
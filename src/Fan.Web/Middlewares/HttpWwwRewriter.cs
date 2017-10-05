using Fan.Enums;
using Fan.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace Fan.Web.Middlewares
{
    public class HttpWwwRewriter : IHttpWwwRewriter
    {
        private ILogger<HttpWwwRewriter> _logger;
        private bool _schemeRequireUpdate;
        private bool _hostRequireWwwAddition;
        private bool _hostRequireWwwRemoval;

        public HttpWwwRewriter(ILogger<HttpWwwRewriter> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Returns true if request url requires a url rewrite based on appsettings, 
        /// the out param url will be the new url to redirect to.
        /// </summary>
        /// <param name="appSettings"></param>
        /// <param name="requestUrl"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public bool ShouldRewrite(AppSettings appSettings, string requestUrl, out string url)
        {
            Uri uri = new Uri(requestUrl);
            string host = uri.Authority; // host with port

            // if useHttps is set to false, but the user is using https, that's ok
            _schemeRequireUpdate = appSettings.UseHttps && uri.Scheme != "https";

            // add www if domain does not start with www and domain has only 1 dot, 
            // so yoursite.azurewebsites.net and localhost:1234 would disqualify it
            _hostRequireWwwAddition = appSettings.PreferredDomain == EPreferredDomain.Www &&
                                      !host.StartsWith("www.") &&
                                      host.Count(s => s == '.') == 1;

            // remove www if domain starts with www
            _hostRequireWwwRemoval = appSettings.PreferredDomain == EPreferredDomain.NonWww && host.StartsWith("www.");

            url = GetUrl(uri.Scheme, host, uri.PathAndQuery, uri.Fragment);
            return _schemeRequireUpdate || _hostRequireWwwAddition || _hostRequireWwwRemoval;
        }

        private string GetUrl(string scheme, string host, string pathAndQuery, string fragment)
        {
            scheme = _schemeRequireUpdate ? "https" : scheme;

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
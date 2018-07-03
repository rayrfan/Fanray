using Fan.Settings;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace Fan.Web.Infrastructure
{
    public class PreferredDomainRewriter : IPreferredDomainRewriter
    {
        /// <summary>
        /// Rewrties url to preferred domain if necessary.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequest"/> object.</param>
        /// <param name="preferredDomain"></param>
        /// <returns>The new URL to redirect to as an out parameter.</returns>
        public string Rewrite(HttpRequest request, EPreferredDomain preferredDomain)
        {
            if (preferredDomain == EPreferredDomain.Auto)
            {
                return null;
            }

            string host = request.Host.Value.ToLower();

            // add "www" 
            // domain needs to have 1 dot so "blog.mysite.com" or "localhost:1234" would not rewrite
            if (preferredDomain == EPreferredDomain.Www 
                && !host.StartsWith("www.") 
                && host.Count(c => c == '.') == 1)
            {
                return $"{request.Scheme}://www.{request.Host}{request.Path}{request.QueryString}";
            }

            // remove "www"
            if (preferredDomain == EPreferredDomain.NonWww 
                && host.StartsWith("www."))
            {
                host = request.Host.Value.Remove(0, 4);
                return $"{request.Scheme}://{host}{request.Path}{request.QueryString}";
            }

            return null;
        }
    }
}
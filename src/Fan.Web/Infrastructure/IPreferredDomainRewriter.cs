using Fan.Settings;
using Microsoft.AspNetCore.Http;

namespace Fan.Web.Infrastructure
{
    public interface IPreferredDomainRewriter
    {
        /// <summary>
        /// Rewrties URL to preferred domain and returns new URL if a redirect is necessary or null
        /// if no redirect is needed.
        /// </summary>
        /// <param name="request">The incoming request.</param>
        /// <param name="preferredDomain">The <see cref="EPreferredDomain"/> of <see cref="AppSettings"/>.</param>
        /// <returns>The new URL to redirect to as an out parameter.</returns>
        string Rewrite(HttpRequest request, EPreferredDomain preferredDomain);
    }
}
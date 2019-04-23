using Fan.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace Fan.Web.Middlewares
{
    /// <summary>
    /// A middleware that does preferred domain URL forward based on user option in <see cref="AppSettings.PreferredDomain"/>.
    /// </summary>
    /// <remarks>
    /// It does a 301 permanent redirect as recommended by Google for preferred domain https://support.google.com/webmasters/answer/44231
    /// </remarks>
    public class PreferredDomainMiddleware
    {
        private readonly RequestDelegate _next;
        private ILogger<PreferredDomainMiddleware> _logger;

        /// <summary>
        /// Initializes the PreferredDomainMiddleware.
        /// </summary>
        /// <param name="next"></param>
        /// <param name="loggerFactory"></param>
        public PreferredDomainMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = loggerFactory.CreateLogger<PreferredDomainMiddleware>();
        }

        /// <summary>
        /// Invokes the PreferredDomainMiddleware.
        /// </summary>
        /// <param name="context">The http context.</param>
        /// <param name="settings"><see cref="AppSettings"/></param>
        /// <param name="rewriter"><see cref="IPreferredDomainRewriter"/></param>
        /// <returns></returns>
        public Task Invoke(HttpContext context, IOptionsSnapshot<AppSettings> settings, IPreferredDomainRewriter rewriter)
        {
            var url = rewriter.Rewrite(context.Request, settings.Value.PreferredDomain);
            if (url == null)
            {
                // no rewrite is needed
                return _next(context);
            }

            _logger.LogInformation("RewriteUrl: {@RewriteUrl}", url);
            //context.Response.Headers[HeaderNames.Location] = url;
            //context.Response.StatusCode = StatusCodes.Status301MovedPermanently;
            context.Response.Redirect(url, permanent: true);

            return Task.CompletedTask;
        }
    }
}
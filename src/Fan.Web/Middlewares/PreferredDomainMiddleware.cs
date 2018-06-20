using Fan.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
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
        /// <param name="context"></param>
        /// <param name="rewriter"></param>
        /// <returns></returns>
        public Task Invoke(HttpContext context, IPreferredDomainRewriter rewriter)
        {
            // has to locate service instead of inject in for appsettings update to be picked up in middleware automatically
            var settings = context.RequestServices.GetService<IOptionsSnapshot<AppSettings>>().Value;
            _logger.LogDebug("PreferredDomain {@PreferredDomain}", settings.PreferredDomain);
            
            // if need to rewrite
            if (rewriter.ShouldRewrite(settings, context.Request.GetDisplayUrl(), out string url))
            {
                _logger.LogInformation("RewriteUrl: {@RewriteUrl}", url);

                context.Response.Headers[HeaderNames.Location] = url;
                context.Response.StatusCode = 301;
                //context.Response.Redirect(url, permanent: true);
                return Task.CompletedTask;
            }

            // if no need to rewrite
            return _next(context);
        }
    }
}
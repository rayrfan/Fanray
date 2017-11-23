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
    public class HttpWwwRewriteMiddleware
    {
        private readonly RequestDelegate _next;
        private ILogger<HttpWwwRewriteMiddleware> _logger;

        public HttpWwwRewriteMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = loggerFactory.CreateLogger<HttpWwwRewriteMiddleware>();
        }

        public Task Invoke(HttpContext context, IHttpWwwRewriter helper)
        {
            // has to locate service instead of inject in for appsettings update to be picked up in middleware automatically
            var settings = context.RequestServices.GetService<IOptionsSnapshot<AppSettings>>().Value;

            _logger.LogDebug("PreferredDomain {@PreferredDomain}", settings.PreferredDomain);
            _logger.LogDebug("UseHttps {@UseHttps}", settings.UseHttps);

            if (helper.ShouldRewrite(settings, context.Request.GetDisplayUrl(), out string url))
            {
                _logger.LogInformation("RewriteUrl: {@RewriteUrl}", url);

                context.Response.Headers[HeaderNames.Location] = url;
                context.Response.StatusCode = 301;
                context.Response.Redirect(url, permanent: true);
                return Task.CompletedTask;
            }

            return _next(context);
        }
    }
}
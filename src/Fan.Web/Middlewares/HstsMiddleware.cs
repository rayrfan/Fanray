using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Fan.Web.Middlewares
{
    /// <summary>
    /// Middleware that sets HSTS response header for ensuring subsequent requests are made over HTTPS only.
    /// See https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Strict-Transport-Security for more details.
    /// </summary>
    /// <remarks>
    /// Code borrowed from https://github.com/aspnet/live.asp.net
    /// </remarks>
    public class HstsMiddleware
    {
        private static readonly string _hstsHeaderName = "Strict-Transport-Security";
        private readonly RequestDelegate _next;
        private readonly HstsOptions _options;
        private readonly string _headerValue;
        private readonly ILogger<HstsMiddleware> _logger;

        public HstsMiddleware(RequestDelegate next, HstsOptions options, ILogger<HstsMiddleware> logger)
        {
            _next = next;
            _options = options;
            _logger = logger;
            _headerValue = FormatHeader(options);
        }

        public Task Invoke(HttpContext httpContext)
        {
            if (httpContext.Response.HasStarted)
            {
                _logger.LogInformation("HSTS response header cannot be set as response writing has already started.");
                return _next(httpContext);
            }

            if (!_options.EnableLocalhost && string.Equals(httpContext.Request.Host.Host, "localhost", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogDebug("HSTS response header will not be set for localhost.");
                return _next(httpContext);
            }

            if (!httpContext.Request.IsHttps)
            {
                _logger.LogDebug("HSTS response header will not be set as the scheme is not HTTPS.");
                return _next(httpContext);
            }

            if (httpContext.Request.Headers.ContainsKey(_hstsHeaderName))
            {
                _logger.LogDebug("HSTS response header is already set: {headerValue}", httpContext.Request.Headers[_hstsHeaderName]);
                return _next(httpContext);
            }

            _logger.LogDebug("Adding HSTS response header: {headerValue}", _headerValue);
            httpContext.Response.Headers.Add(_hstsHeaderName, _headerValue);

            return _next(httpContext);
        }

        private string FormatHeader(HstsOptions options)
        {
            var headerValue = "max-age=" + _options.MaxAge.TotalSeconds;

            if (_options.IncludeSubdomains)
            {
                headerValue += "; includeSubdomains";
            }

            if (_options.Preload)
            {
                headerValue += "; preload";
            }

            return headerValue;
        }
    }
}

using Fan.Settings;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Fan.Web.Infrastructure
{
    /// <summary>
    /// Middleware to setup the site.
    /// </summary>
    public class SetupMiddleware
    {
        private readonly RequestDelegate _next;

        public SetupMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>
        /// Invoke middleware to test if setup needs to happen.
        /// </summary>
        /// <param name="context">The HTTP context</param>
        /// <param name="settingService">The settings service</param>
        /// <returns>Task</returns>
        public async Task Invoke(HttpContext context, ISettingService settingService)
        {
            var coreSettings = await settingService.GetSettingsAsync<CoreSettings>();

            if (!coreSettings.SetupDone)
            {
                var setupUrl = $"{context.Request.Scheme}://{context.Request.Host}/setup";
                var currentUrl = $"{context.Request.Scheme}://{context.Request.Host}{context.Request.Path}";

                // don't redirect to setup if url is setup itself or certain types of files
                string[] exts = { ".ico", ".js", ".css", ".map" };
                if (!currentUrl.Equals(setupUrl, StringComparison.OrdinalIgnoreCase) && 
                    !exts.Any(ext=> currentUrl.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
                {
                    context.Response.Redirect(setupUrl);
                    return;
                }
            }

            // no need to setup
            await _next(context);
        }
    }
}

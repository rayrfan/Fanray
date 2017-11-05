using Fan.Blogs.MetaWeblog;
using Fan.Web.Middlewares;

namespace Microsoft.AspNetCore.Builder
{
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Adds <see cref="MetaWeblogMiddleware"/> to the application's request pipeline.
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseMetablog(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<MetaWeblogMiddleware>();
        }

        /// <summary>
        /// Adds <see cref="HttpWwwRewriteMiddleware"/> for redirect between http / https and 
        /// preferred domain www / nonwww.
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseHttpWwwRewrite(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<HttpWwwRewriteMiddleware>();
        }

        /// <summary>
        /// Add middleware that sets HSTS response header for ensuring subsequent requests are made over HTTPS only.
        /// See https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Strict-Transport-Security for more details.
        /// </summary>
        /// <param name="builder">The <see cref="IApplicationBuilder"/>.</param>
        /// <returns>The <see cref="IApplicationBuilder"/>.</returns>
        public static IApplicationBuilder UseHsts(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<HstsMiddleware>(new HstsOptions());
        }

        /// <summary>
        /// Add middleware that sets HSTS response header for ensuring subsequent requests are made over HTTPS only.
        /// See https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Strict-Transport-Security for more details.
        /// </summary>
        /// <param name="builder">The <see cref="IApplicationBuilder"/>.</param>
        /// <param name="options">The <see cref="HstsOptions"/> to use.</param>
        /// <returns>The <see cref="IApplicationBuilder"/>.</returns>
        public static IApplicationBuilder UseHsts(this IApplicationBuilder builder, HstsOptions options)
        {
            return builder.UseMiddleware<HstsMiddleware>(options);
        }
    }
}

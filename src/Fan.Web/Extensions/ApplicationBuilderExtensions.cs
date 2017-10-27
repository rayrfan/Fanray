using Fan.Web.MetaWeblog;
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
    }
}

using Fan.Blogs.MetaWeblog;
using Fan.Web.Infrastructure;

namespace Microsoft.AspNetCore.Builder
{
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Adds <see cref="MetaWeblogMiddleware"/> to the application's request pipeline.
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseMetablog(this IApplicationBuilder app)
        {
            return app.UseMiddleware<MetaWeblogMiddleware>();
        }

        /// <summary>
        /// Adds <see cref="PreferredDomainMiddleware"/> for redirecting requests to the preferred domain www or nonwww.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/> instance this method extends.</param>
        /// <returns>The <see cref="IApplicationBuilder"/> for preferred domain rewrite.</returns>
        public static IApplicationBuilder UsePreferredDomain(this IApplicationBuilder app)
        {
            return app.UseMiddleware<PreferredDomainMiddleware>();
        }

        /// <summary>
        /// Adds <see cref="SetupMiddleware"/> to check if site needs to be setup.
        /// </summary>
        /// <param name="app">Builder for configuring an application's request pipeline</param>
        public static IApplicationBuilder UseSetup(this IApplicationBuilder app)
        {
            return app.UseMiddleware<SetupMiddleware>();
        }
    }
}

using Fan.Settings;

namespace Fan.Web.Middlewares
{
    public interface IPreferredDomainRewriter
    {
        /// <summary>
        /// Returns true if request url requires a url rewrite based on appsettings, 
        /// the out param url will be the new url to redirect to.
        /// </summary>
        /// <param name="appSettings"></param>
        /// <param name="requestUrl"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        bool ShouldRewrite(AppSettings appSettings, string requestUrl, out string url);
    }
}
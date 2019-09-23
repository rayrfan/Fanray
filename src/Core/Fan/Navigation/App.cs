using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace Fan.Navigation
{
    /// <summary>
    /// The different apps Fanray has.
    /// </summary>
    public class App : INavProvider
    {
        public const int BLOG_APP_ID = 1;
        public const string BLOG_APP_NAME = "Blog";
        public const string BLOG_APP_URL = "blog";

        public static readonly List<Nav> AppNavs = new List<Nav>
        {
            new Nav { Id = BLOG_APP_ID, Text = BLOG_APP_NAME, Url = $"/{BLOG_APP_URL}", Type = ENavType.App },
        };

        public bool CanProvideNav(ENavType type) => type == ENavType.App;

        public async Task<string> GetNavUrlAsync(int id)
        {
            var appNav = await AppNavs.ToAsyncEnumerable().Single(a => a.Id == id);
            return appNav.Url;
        }
    }
}

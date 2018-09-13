using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Fan.Web.Pages.Admin
{
    /// <summary>
    /// The admin index page, it redirects.
    /// </summary>
    public class IndexModel : PageModel
    {
        /// <summary>
        /// Redirects to "admin/posts".
        /// </summary>
        /// <returns></returns>
        public IActionResult OnGet()
        {
            return Redirect($"admin/posts");
        }
    }
}
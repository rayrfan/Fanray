using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Fan.Web.Areas.Admin.Pages
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
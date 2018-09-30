using System.ComponentModel.DataAnnotations;

namespace Fan.Web.ViewModels
{
    /// <summary>
    /// The view model for Pages/Login.cshtml.
    /// </summary>
    public class LoginViewModel
    {
        /// <summary>
        /// Either email or username.
        /// </summary>
        [Required]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }
}

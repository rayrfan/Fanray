using System.ComponentModel.DataAnnotations;

namespace Fan.Web.Models.AuthViewModels
{
    /// <summary>
    /// The view model for Pages/Login.cshtml.
    /// </summary>
    public class LoginVM
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

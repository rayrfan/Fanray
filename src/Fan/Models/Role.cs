using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Fan.Models
{
    public class Role : IdentityRole<int>
    {
        public const string ADMINISTRATOR_ROLE = "Administrator";

        /// <summary>
        /// A brief description of what the role is about.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets if the role is a system role.
        /// </summary>
        [Required]
        public bool IsSystemRole { get; set; }
    }
}

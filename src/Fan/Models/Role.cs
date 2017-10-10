using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Fan.Models
{
    public class Role : IdentityRole<int>
    {
        /// <summary>
        /// Gets or sets if the role is a system role.
        /// </summary>
        [Required]
        public bool IsSystemRole { get; set; }
    }
}

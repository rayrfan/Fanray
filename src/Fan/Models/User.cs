using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Fan.Models
{
    // Add profile data for application users by adding properties to the User class
    public class User : IdentityUser<int>
    {
        /// <summary>
        /// The friendly name to display on posts.
        /// </summary>
        [Required]
        [StringLength(maximumLength: 256)]
        public string DisplayName { get; set; }
    }
}

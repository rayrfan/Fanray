using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;

namespace Fan.Models
{
    public class User : IdentityUser<int>
    {
        public User()
        {
            CreatedOn = DateTimeOffset.UtcNow;
        }

        /// <summary>
        /// When the user was created.
        /// </summary>
        public DateTimeOffset CreatedOn { get; set; }

        /// <summary>
        /// The friendly name to display on posts.
        /// </summary>
        [Required]
        [StringLength(maximumLength: 256)]
        public string DisplayName { get; set; }

        /// <summary>
        /// The length of user's total upload file in byte.
        /// </summary>
        public long UploadLength { get; set; }
    }
}

using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Fan.Web.Models.BlogViewModels
{
    public class SetupViewModel
    {
        public SetupViewModel()
        {
            // https://docs.microsoft.com/en-us/aspnet/core/mvc/views/working-with-forms#the-select-tag-helper
            TimeZones = new List<SelectListItem>();
            foreach (var tz in TimeZoneInfo.GetSystemTimeZones())
            {
                TimeZones.Add(new SelectListItem() { Value = tz.Id, Text = tz.DisplayName });
            }
            TimeZoneId = "Pacific Standard Time";
        }


        [Required]
        [Display(Name = "Blog Name")]
        public string Title { get; set; }

        public string TimeZoneId { get; set; }

        [Display(Name = "Time Zone")]
        public List<SelectListItem> TimeZones { get; set; }

        [Required]
        [Display(Name = "Username")]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 4)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}

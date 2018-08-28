using Fan.Blog.Services;
using Fan.Exceptions;
using Fan.Models;
using Fan.Settings;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fan.Web.Pages
{
    public class SetupModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ISettingService _settingSvc;
        private readonly IBlogService _blogSvc;
        private readonly ILogger<SetupModel> _logger;

        public SetupModel(
            UserManager<User> userManager,
            RoleManager<Role> roleManager,
            SignInManager<User> signInManager,
            IBlogService blogService,
            ISettingService settingService,
            ILogger<SetupModel> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _blogSvc = blogService;
            _settingSvc = settingService;
            _logger = logger;
        }

        public string Title { get; set; }
        public string TimeZoneId { get; set; }
        public List<SelectListItem> TimeZones { get; set; }
        public string TimeZonesJson { get; set; }
        public string DisplayName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

        /// <summary>
        /// Initializes setup and if setup has been done redirect to blog index page.
        /// </summary>
        public async Task<IActionResult> OnGet()
        {
            var coreSettings = await _settingSvc.GetSettingsAsync<CoreSettings>();
            if (coreSettings.SetupDone)
            {
                return RedirectToAction("Index", "Blog");
            }

            // https://docs.microsoft.com/en-us/aspnet/core/mvc/views/working-with-forms#the-select-tag-helper
            TimeZones = new List<SelectListItem>();
            foreach (var tz in TimeZoneInfo.GetSystemTimeZones())
            {
                TimeZones.Add(new SelectListItem() { Value = tz.Id, Text = tz.DisplayName });
            }
            TimeZoneId = "UTC";
            TimeZonesJson = JsonConvert.SerializeObject(TimeZones);

            return Page();
        }

        /// <summary>
        /// POST to create a new category.
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        public async Task<IActionResult> OnPostAsync([FromBody] SetupModel model)
        {
            try
            {
                _logger.LogInformation("Fanray Setup Begins");

                var validator = new SetupValidator();
                var valResult = await validator.ValidateAsync(model);
                if (!valResult.IsValid)
                {
                    throw new FanException($"Failed to create blog.", valResult.Errors);
                }

                // user and role
                var user = new User
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    DisplayName = model.DisplayName
                };
                var role = new Role
                {
                    Name = Role.ADMINISTRATOR_ROLE,
                    IsSystemRole = true,
                    Description = "An Administrator has full power over the site and can do everything."
                };

                IdentityResult result = IdentityResult.Success;

                // create user if not found
                if (await _userManager.FindByEmailAsync(model.Email) == null)
                {
                    result = await _userManager.CreateAsync(user, model.Password);
                }

                // create Admin role if not found
                if (result.Succeeded)
                {
                    _logger.LogInformation("{@User} account created.", user);

                    if (!await _roleManager.RoleExistsAsync(Role.ADMINISTRATOR_ROLE))
                        result = await _roleManager.CreateAsync(role);
                }

                // assign Admin role to user, if not assigned already
                if (result.Succeeded)
                {
                    _logger.LogInformation("{@Role} role created.", role);

                    // get the actual user object before look up IsInRole
                    user = await _userManager.FindByEmailAsync(user.Email);
                    if (!await _userManager.IsInRoleAsync(user, Role.ADMINISTRATOR_ROLE))
                        result = await _userManager.AddToRoleAsync(user, Role.ADMINISTRATOR_ROLE);
                }

                if (result.Succeeded)
                {
                    _logger.LogInformation("{@Role} role assigned to user {@User}.", role, user);

                    // create core settings
                    await _settingSvc.UpsertSettingsAsync(new CoreSettings
                    {
                        Title = model.Title,
                        TimeZoneId = model.TimeZoneId,
                        SetupDone = true, // setup is done
                    });
                    _logger.LogInformation("Setup is done, CoreSettings created!");

                    // sign-in user
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    _logger.LogInformation("User has been signed in.");

                    // create blog initial posts and settings
                    await _blogSvc.SetupAsync();

                    return new JsonResult(true);
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return BadRequest(result.Errors);
            }
            catch (FanException ex)
            {
                return BadRequest(ex.ValidationFailures);
            }
        }
    }

    public class SetupValidator : AbstractValidator<SetupModel>
    {
        /// <summary>
        /// DisplayName or UserName should be at least 2 chars min.
        /// </summary>
        public const int NAME_MINLENGTH = 2;
        /// <summary>
        /// UserName should be no more than 20 chars max.
        /// </summary>
        public const int USERNAME_MAXLENGTH = 20;
        /// <summary>
        /// DisplayName should be no more than 32 chars max.
        /// </summary>
        public const int DISPLAYNAME_MAXLENGTH = 32;
        /// <summary>
        /// Password should be at least 8 chars min.
        /// </summary>
        public const int PASSWORD_MINLENGTH = 8;
        /// <summary>
        /// UserName can only contain alphanumeric, dash and underscore.
        /// </summary>
        public const string USERNAME_REGEX = @"^[a-zA-Z0-9-_]+$";
        /// <summary>
        /// Reserved keywords username cannot use.
        /// </summary>
        public static string[] USERNAME_RESERVED = new string[]
        {
            "admin", "anonymous", "api", "account", "about",
            "blog", "blogs",
            "contact",
            "home", "help",
            "login", "logout",
            "manage",
            "privacy", "page", "pages",
            "register",
            "system",
            "terms",
            "user", "users",
            "signin-google"
        };

        public SetupValidator()
        {
            // Email
            RuleFor(s => s.Email).EmailAddress();

            // UserName
            RuleFor(s => s.UserName)
                .NotEmpty()
                .Length(NAME_MINLENGTH, USERNAME_MAXLENGTH)
                .Matches(USERNAME_REGEX)
                .Must(title => !USERNAME_RESERVED.Contains(title, StringComparer.CurrentCultureIgnoreCase))
                .WithMessage(s => $"Username '{s.UserName}' is not available.");

            // DisplayName
            RuleFor(s => s.DisplayName)
                .NotEmpty()
                .Length(NAME_MINLENGTH, DISPLAYNAME_MAXLENGTH);

            // Password
            RuleFor(s => s.Password).MinimumLength(PASSWORD_MINLENGTH);
        }
    }
}
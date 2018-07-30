using Fan.Blog.Services;
using Fan.Models;
using Fan.Settings;
using Fan.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Fan.Web.Controllers
{
    public class SetupController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ISettingService _settingSvc;
        private readonly IBlogService _blogSvc;
        private readonly ILogger<HomeController> _logger;

        public SetupController(
            UserManager<User> userManager,
            RoleManager<Role> roleManager,
            SignInManager<User> signInManager,
            IBlogService blogService,
            ISettingService settingService,
            ILogger<HomeController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _blogSvc = blogService;
            _settingSvc = settingService;
            _logger = logger;
        }

        /// <summary>
        /// Setup page.
        /// </summary>
        /// <remarks>
        /// If there is already admin user, then update <see cref="CoreSettings.SetupDone"/> and redirect to blog index page.
        /// </remarks>
        /// <returns></returns>
        public async Task<IActionResult> Index()
        {
            // if there is admin user, then redirect to blog
            if ((await _userManager.GetUsersInRoleAsync(Role.ADMINISTRATOR_ROLE)).Count() > 0)
            {                
                await _settingSvc.UpsertSettingsAsync(new CoreSettings { SetupDone = true });
                _logger.LogInformation("Setup is done, CoreSettings updated!");

                return RedirectToAction("Index", "Blog");
            }

            return View(new SetupViewModel());
        }

        /// <summary>
        /// Sets up site, blog, creates user, role, settings and default blog category.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(SetupViewModel model)
        {
            if (ModelState.IsValid)
            {
                _logger.LogInformation("Fanray Setup Begins");

                // user with email as username
                var user = new User { UserName = model.Email, Email = model.Email, DisplayName = model.DisplayName };
                var role = new Role
                {
                    Name = Role.ADMINISTRATOR_ROLE,
                    IsSystemRole = true,
                    Description = "An Administrator has full power over the site and can do everything."
                };

                // create user
                var result = await _userManager.CreateAsync(user, model.Password);

                // create Admin role
                if (result.Succeeded)
                {
                    _logger.LogInformation("{@User} account created with password.", user);
                    if (!await _roleManager.RoleExistsAsync(Role.ADMINISTRATOR_ROLE))
                        result = await _roleManager.CreateAsync(role);
                }

                // assign Admin role to user
                if (result.Succeeded)
                {
                    _logger.LogInformation("{@Role} created.", role);
                    result = await _userManager.AddToRoleAsync(user, Role.ADMINISTRATOR_ROLE);
                }

                if (result.Succeeded)
                {
                    _logger.LogInformation("{@Role} assigned to {@User}.", role, user);

                    // sign-in user
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    _logger.LogInformation("User has been signed in.");

                    // create blog initial posts and settings
                    await _blogSvc.SetupAsync(model.DisqusShortname);

                    // create core settings
                    await _settingSvc.UpsertSettingsAsync(new CoreSettings
                    {
                        Title = model.Title,
                        Tagline = model.Tagline,
                        TimeZoneId = model.TimeZoneId,
                        GoogleAnalyticsTrackingID = model.GoogleAnalyticsTrackingID.IsNullOrWhiteSpace() ? null : model.GoogleAnalyticsTrackingID.Trim(),
                        SetupDone = true, // setup is done
                    });
                    _logger.LogInformation("Setup is done, CoreSettings created!");

                    return RedirectToAction("Index", "Blog");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }
    }
}
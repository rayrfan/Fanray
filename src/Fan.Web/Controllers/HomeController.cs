using Fan.Blogs.Controllers;
using Fan.Blogs.Enums;
using Fan.Blogs.Helpers;
using Fan.Blogs.Models;
using Fan.Blogs.Services;
using Fan.Exceptions;
using Fan.Models;
using Fan.Settings;
using Fan.Web.Models;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Fan.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ISettingService _settingSvc;
        private readonly IBlogService _blogSvc;
        private readonly ILogger<HomeController> _logger;

        public HomeController(
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

        public IActionResult Index => RedirectToAction(nameof(BlogController.Index), "Blog");

        /// <summary>
        /// Setup the site and blog, if already setup redirect to blog index page.
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Setup()
        {
            if (await _settingSvc.SettingsExist())
                return RedirectToAction(nameof(BlogController.Index), "Blog");

            return View(new SetupViewModel());
        }

        /// <summary>
        /// Sets up site, blog, creates user, role, settings and default blog category.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Setup(SetupViewModel model)
        {
            if (ModelState.IsValid)
            {
                _logger.LogInformation("Fanray Setup Begins");

                // user with email as username
                var user = new User { UserName = model.Email, Email = model.Email, DisplayName = model.DisplayName };
                var adminRole = "Administrator";
                var role = new Role
                {
                    Name = adminRole,
                    IsSystemRole = true,
                    Description = "An Administrator has full power over the site and can do everything."
                };

                // create user
                var result = await _userManager.CreateAsync(user, model.Password);

                // create Admin role
                if (result.Succeeded)
                {
                    _logger.LogInformation("{@User} account created with password.", user);
                    if (!await _roleManager.RoleExistsAsync(adminRole))
                        result = await _roleManager.CreateAsync(role);
                }

                // assign Admin role to user
                if (result.Succeeded)
                {
                    _logger.LogInformation("{@Role} created.", role);
                    result = await _userManager.AddToRoleAsync(user, adminRole);
                }

                if (result.Succeeded)
                {
                    _logger.LogInformation("{@Role} assigned to {@User}.", role, user);

                    // sign-in user
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    _logger.LogInformation("User has been signed in.");

                    // create site settings
                    await _settingSvc.UpsertSettingsAsync(new CoreSettings
                    {
                        Title = model.Title,
                        Tagline = model.Tagline,
                        TimeZoneId = model.TimeZoneId,
                        GoogleAnalyticsTrackingID = model.GoogleAnalyticsTrackingID.IsNullOrWhiteSpace() ? null : model.GoogleAnalyticsTrackingID.Trim(),
                    });
                    _logger.LogInformation("CoreSettings created.");

                    // create blog settings
                    await _settingSvc.UpsertSettingsAsync(new BlogSettings
                    {
                        CommentProvider = model.DisqusShortname.IsNullOrWhiteSpace() ? ECommentProvider.Fanray : ECommentProvider.Disqus,
                        DisqusShortname = model.DisqusShortname.IsNullOrWhiteSpace() ? null : model.DisqusShortname.Trim(),
                    });
                    _logger.LogInformation("BlogSettings created.");

                    // create welcome post and default category
                    await _blogSvc.CreatePostAsync(new BlogPost
                    {
                        CategoryTitle = BlogConst.DEFAULT_CATEGORY,
                        TagTitles = null,
                        Title = BlogConst.WELCOME_POST_TITLE,
                        Body = BlogConst.WELCOME_POST_BODY,
                        UserId = 1,
                        Status = EPostStatus.Published,
                        CommentStatus = ECommentStatus.AllowComments,
                        CreatedOn = DateTimeOffset.Now,
                    });
                    _logger.LogInformation("Welcome post and default category created.");
                    _logger.LogInformation("Blog Setup completes.");

                    return RedirectToAction("Index", "Blog");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        /// <summary>
        /// 404 comes here.
        /// </summary>
        /// <param name="statusCode"></param>
        /// <returns></returns>
        /// <remarks>
        /// 500 caused by an unhandled exception goes to <see cref="Error"/> action.
        /// </remarks>
        [HttpGet("/Home/ErrorCode/{statusCode}")]
        public IActionResult ErrorCode(int statusCode) => statusCode == 404 ? View("404") : View("Error");

        /// <summary>
        /// Friendly error page in Production, in Development the DeveloperExceptionPage will be 
        /// used instead coming here.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Unhandled FanException comes here and its message will be displayed. The 500 or other 
        /// unhandled exceptions will come here also, a hard coded message is displayed on Error.cshtml.
        /// 
        /// For actions that need to display message on its page, i.e. a form that fails validation
        /// should catch FanException to display its message on its page.
        /// </remarks>
        public IActionResult Error()
        {
            var feature = HttpContext.Features.Get<IExceptionHandlerFeature>();
            var error = feature?.Error;

            // FanException occurred unhandled
            if (error !=null && error is FanException)
            {
                return View("Error", error.Message);
            }

            // 500 or exception other than FanException occurred unhandled
            return View();
        }
    }
}

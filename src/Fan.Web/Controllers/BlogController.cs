using Fan.Blogs.Enums;
using Fan.Blogs.Helpers;
using Fan.Blogs.Models;
using Fan.Blogs.Services;
using Fan.Models;
using Fan.Services;
using Fan.Web.Models.BlogViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Fan.Web.Controllers
{
    public class BlogController : Controller
    {
        private readonly IBlogService _blogSvc;
        private readonly ISettingService _settingSvc;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ILogger<BlogController> _logger;

        public BlogController(IBlogService blogService,
            ISettingService settingService,
             UserManager<User> userManager,
             RoleManager<Role> roleManager,
            SignInManager<User> signInManager,
            ILogger<BlogController> logger)
        {
            _blogSvc = blogService;
            _settingSvc = settingService;
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var settings = await _settingSvc.GetSettingsAsync<SiteSettings>();
            if (settings == null)
                return RedirectToAction("Setup");

            var posts = await _blogSvc.GetPostsAsync(1);
            return View(posts);
        }

        /// <summary>
        /// Setup blog page, if already setup redirect to blog index page.
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Setup()
        {
            var settings = await _settingSvc.GetSettingsAsync<SiteSettings>();
            if (settings != null)
                return RedirectToAction("Index");

            return View(new SetupViewModel());
        }

        /// <summary>
        /// Sets up the blog, creates user, role, blogsettings and default category.
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
                var adminRole = "Admin";
                var role = new Role { Name = adminRole, IsSystemRole = true };

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

                    // create site and blog settings
                    await _settingSvc.CreateSettingsAsync(new SiteSettings
                    {
                        Title = model.Title,
                        Tagline = model.Tagline,
                        TimeZoneId = model.TimeZoneId
                    });
                    await _settingSvc.CreateSettingsAsync(new BlogSettings());
                    _logger.LogInformation("Site and Blog Settings created.");

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
                        CreatedOn = DateTime.Now,
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


        /// <summary>
        /// Returns content of the RSD file which will tell where the MetaWeblog API endpoint is.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// https://en.wikipedia.org/wiki/Really_Simple_Discovery
        /// </remarks>
        public IActionResult Rsd()
        {
            var rootUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}";
            return View("Rsd", rootUrl);
        }

        /// <summary>
        /// Returns viewing of a single post.
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="day"></param>
        /// <param name="slug"></param>
        /// <returns></returns>
        public async Task<IActionResult> Post(int year, int month, int day, string slug)
        {
            var post = await _blogSvc.GetPostAsync(slug, year, month, day);
            return View(post);
        }

        public async Task<IActionResult> Category(string slug)
        {
            var cat = await _blogSvc.GetCategoryAsync(slug);
            var posts = await _blogSvc.GetPostsForCategoryAsync(slug, 1);

            return View(new Tuple<Category, BlogPostList>(cat, posts));
        }

        public async Task<IActionResult> Tag(string slug)
        {
            var tag = await _blogSvc.GetTagAsync(slug);
            var posts = await _blogSvc.GetPostsForTagAsync(slug, 1);

            return View(new Tuple<Tag, BlogPostList>(tag, posts));
        }
    }
}
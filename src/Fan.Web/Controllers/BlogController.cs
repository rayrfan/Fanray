using Fan.Enums;
using Fan.Helpers;
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
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ILogger<BlogService> _logger;

        public BlogController(IBlogService blogService,
             UserManager<User> userManager,
            SignInManager<User> signInManager,
            ILogger<BlogService> logger)
        {
            _blogSvc = blogService;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var settings = await _blogSvc.GetSettingsAsync();
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
            var settings = await _blogSvc.GetSettingsAsync();
            if (settings != null)
                return RedirectToAction("Index");

            return View(new SetupViewModel());
        }

        /// <summary>
        /// Setting up the blog, create user, create blogsettings and default category.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Setup(SetupViewModel model)
        {
            if (ModelState.IsValid)
            {
                // create user
                var user = new User { UserName = model.UserName, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Blog Setup begins.");
                    _logger.LogInformation("User account created with password.");

                    //// sign-in user
                    //await _signInManager.SignInAsync(user, isPersistent: false);
                    //_logger.LogInformation("User has been signed in.");

                    // create blog settings
                    await _blogSvc.CreateSettingsAsync(new BlogSettings {
                        Title = model.Title,
                        TimeZoneId = model.TimeZoneId
                    });
                    _logger.LogInformation("BlogSettings created.");

                    // create welcome post and default category
                    await _blogSvc.CreatePostAsync(new BlogPost
                    {
                        CategoryTitle = Const.DEFAULT_CATEGORY,
                        TagTitles = null,
                        Title = Const.WELCOME_POST_TITLE,
                        Body = Const.WELCOME_POST_BODY,
                        UserName = model.UserName,
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
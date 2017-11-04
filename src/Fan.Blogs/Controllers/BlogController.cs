using Fan.Blogs.Enums;
using Fan.Blogs.Helpers;
using Fan.Blogs.Models;
using Fan.Blogs.Services;
using Fan.Blogs.ViewModels;
using Fan.Models;
using Fan.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.SyndicationFeed;
using Microsoft.SyndicationFeed.Rss;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml;

namespace Fan.Blogs.Controllers
{
    public class BlogController : Controller
    {
        private readonly IBlogService _blogSvc;
        private readonly ISettingService _settingSvc;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ILogger<BlogController> _logger;
        private readonly IDistributedCache _cache;

        public BlogController(
            IBlogService blogService,
            ISettingService settingService,
            UserManager<User> userManager,
            RoleManager<Role> roleManager,
            SignInManager<User> signInManager,
            IDistributedCache cache,
            ILogger<BlogController> logger)
        {
            _blogSvc = blogService;
            _settingSvc = settingService;
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _cache = cache;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var settings = await _settingSvc.GetSettingsAsync<SiteSettings>();
            if (settings == null)
                return RedirectToAction("Setup");

            var posts = await _blogSvc.GetPostsAsync(1);
            var blogSettings = await _settingSvc.GetSettingsAsync<BlogSettings>();
            var vm = new BlogPostListViewModel(posts, blogSettings, Request);
            return View(vm);
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

                    // create site and blog settings
                    await _settingSvc.CreateSettingsAsync(new SiteSettings
                    {
                        Title = model.Title,
                        Tagline = model.Tagline,
                        TimeZoneId = model.TimeZoneId
                    });
                    await _settingSvc.CreateSettingsAsync(new BlogSettings
                    {
                        CommentProvider = model.DisqusShortname.IsNullOrWhiteSpace() ? ECommentProvider.Fanray : ECommentProvider.Disqus,
                        DisqusShortname = model.DisqusShortname.IsNullOrWhiteSpace() ? null : model.DisqusShortname.Trim(),
                    });
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
            var blogPost = await _blogSvc.GetPostAsync(slug, year, month, day);
            var blogSettings = await _settingSvc.GetSettingsAsync<BlogSettings>();
            var vm = new BlogPostViewModel(blogPost, blogSettings, Request);
            return View(vm);
        }

        public async Task<IActionResult> PostPerma(int id)
        {
            var post = await _blogSvc.GetPostAsync(id);
            return RedirectToAction("Post", new { post.CreatedOn.Year, post.CreatedOn.Month, post.CreatedOn.Day, post.Slug});
        }

        public async Task<IActionResult> Category(string slug)
        {
            var cat = await _blogSvc.GetCategoryAsync(slug);
            var posts = await _blogSvc.GetPostsForCategoryAsync(slug, 1);
            var blogSettings = await _settingSvc.GetSettingsAsync<BlogSettings>();
            var vm = new BlogPostListViewModel(posts, blogSettings, Request, cat);
            return View(vm);
        }

        public async Task<IActionResult> Tag(string slug)
        {
            var tag = await _blogSvc.GetTagAsync(slug);
            var posts = await _blogSvc.GetPostsForTagAsync(slug, 1);
            var blogSettings = await _settingSvc.GetSettingsAsync<BlogSettings>();
            var vm = new BlogPostListViewModel(posts, blogSettings, Request, tag);
            return View(vm);
        }

        /// <summary>
        /// Category feed.
        /// </summary>
        /// <param name="slug"></param>
        /// <returns></returns>
        public async Task<ContentResult> CategoryFeed(string slug)
        {
            Category cat = await _blogSvc.GetCategoryAsync(slug);
            var rss = await GetFeed(cat);
            return new ContentResult
            {
                ContentType = "application/xml",
                Content = rss,
                StatusCode = 200
            };
        }

        /// <summary>
        /// Blog main feed.
        /// </summary>
        /// <returns></returns>
        public async Task<ContentResult> Feed()
        {
            var rss = await GetFeed();
            return new ContentResult
            {
                ContentType = "application/xml",
                Content = rss,
                StatusCode = 200
            };
        }

        /// <summary>
        /// Returns the rss xml string for the blog or a blog category. The result is cached for 1 hour.
        /// </summary>
        /// <param name="cat"></param>
        /// <returns></returns>
        private async Task<string> GetFeed(Category cat = null)
        {
            var key = cat == null ? "RssFeed" : $"RssFeed_{cat.Slug}";
            return await _cache.GetAsync(key, new TimeSpan(1, 0, 0), async () =>
            {
                var sw = new StringWriter();
                using (XmlWriter xmlWriter = XmlWriter.Create(sw, new XmlWriterSettings() { Async = true, Indent = true }))
                {
                    var posts = cat == null ?
                                await _blogSvc.GetPostsAsync(1) :
                                await _blogSvc.GetPostsForCategoryAsync(cat.Slug, 1);
                    var siteSettings = await _settingSvc.GetSettingsAsync<SiteSettings>();
                    var blogSettings = await _settingSvc.GetSettingsAsync<BlogSettings>();
                    var vm = new BlogPostListViewModel(posts, blogSettings, Request);

                    var channelTitle = cat == null ? "Fanray" : $"{cat.Title} - Fanray";
                    var channelDescription = siteSettings.Tagline;
                    var channelLink = $"{Request.Scheme}://{Request.Host}";
                    var channelLastPubDate = posts.Count <= 0 ? DateTimeOffset.UtcNow : posts[0].CreatedOn;

                    var writer = new RssFeedWriter(xmlWriter);
                    await writer.WriteTitle(channelTitle);
                    await writer.WriteDescription(channelDescription);
                    await writer.Write(new SyndicationLink(new Uri(channelLink)));
                    await writer.WritePubDate(channelLastPubDate);
                    await writer.WriteGenerator("https://www.fanray.com");

                    foreach (var postVM in vm.BlogPostViewModels)
                    {
                        var post = postVM;
                        var item = new SyndicationItem()
                        {
                            Id = postVM.Permalink, // guid https://www.w3schools.com/xml/rss_tag_guid.asp
                            Title = post.Title,
                            Description = blogSettings.RssShowExcerpt ? post.Excerpt : post.Body,
                            Published = post.CreatedOn,
                        };

                        // link to the post
                        item.AddLink(new SyndicationLink(new Uri(postVM.CanonicalUrl)));

                        // category takes in both cats and tags
                        item.AddCategory(new SyndicationCategory(post.Category.Title));
                        foreach (var tag in post.Tags)
                        {
                            item.AddCategory(new SyndicationCategory(tag.Title));
                        }

                        // https://www.w3schools.com/xml/rss_tag_author.asp
                        // the author tag exposes email  
                        //item.AddContributor(new SyndicationPerson(post.User.DisplayName, post.User.Email));

                        await writer.Write(item);
                    }

                    xmlWriter.Flush();
                }

                return sw.ToString();
            });
        }
    }
}
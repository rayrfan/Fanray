using Fan.Blog.Enums;
using Fan.Blog.Models;
using Fan.Blog.Services.Interfaces;
using Fan.Exceptions;
using Fan.Membership;
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
        private readonly IBlogPostService _blogSvc;
        private readonly ICategoryService _catSvc;
        private readonly ITagService _tagSvc;
        private readonly ILogger<SetupModel> _logger;

        public SetupModel(
            UserManager<User> userManager,
            RoleManager<Role> roleManager,
            SignInManager<User> signInManager,
            IBlogPostService blogService,
            ICategoryService catService,
            ITagService tagService,
            ISettingService settingService,
            ILogger<SetupModel> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _blogSvc = blogService;
            _catSvc = catService;
            _tagSvc = tagService;
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
        public async Task<IActionResult> OnGetAsync()
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
        /// Setting up site.
        /// </summary>
        /// <remarks>
        /// It creates the first user, system roles, assign Administrator role to the user, 
        /// core settings, first blog post and blog settings.
        /// </remarks>
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

                // first user
                var user = new User
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    DisplayName = model.DisplayName
                };              

                IdentityResult result = IdentityResult.Success;

                // create user if not found
                var foundUser = await _userManager.FindByEmailAsync(model.Email);
                if (foundUser == null)
                {
                    result = await _userManager.CreateAsync(user, model.Password);
                }
                else // update username
                {
                    foundUser.UserName = model.UserName;
                    await _userManager.UpdateNormalizedUserNameAsync(foundUser);
                }

                // create system roles
                if (result.Succeeded)
                {
                    _logger.LogInformation("{@User} account created.", user);

                    result = await CreateSystemRolesAsync();
                }

                // assign Administrator role to the user
                if (result.Succeeded)
                {
                    // get the actual user object before look up IsInRole
                    user = await _userManager.FindByEmailAsync(user.Email);

                    if (!await _userManager.IsInRoleAsync(user, Role.ADMINISTRATOR_ROLE))
                        result = await _userManager.AddToRoleAsync(user, Role.ADMINISTRATOR_ROLE);
                }

                if (result.Succeeded)
                {
                    _logger.LogInformation($"{Role.ADMINISTRATOR_ROLE} role has been assigned to user {@User}.", user);

                    // update or create core settings
                    var settings = await _settingSvc.GetSettingsAsync<CoreSettings>();
                    if (settings != null)
                    {
                        settings.Title = model.Title;
                        settings.TimeZoneId = model.TimeZoneId;
                        settings.SetupDone = true;
                        await _settingSvc.UpsertSettingsAsync(settings);
                    }
                    else
                    {
                        await _settingSvc.UpsertSettingsAsync(new CoreSettings
                        {
                            Title = model.Title,
                            TimeZoneId = model.TimeZoneId,
                            SetupDone = true,
                        });
                    }
                    _logger.LogInformation("Setup is done, CoreSettings created!");

                    // sign-in user
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    _logger.LogInformation("User has been signed in.");

                    // setup blog
                    await SetupBlogAsync();

                    return new JsonResult(true);
                }

                return BadRequest(result.Errors.ToList()[0].Description);
            }
            catch (FanException ex)
            {
                return BadRequest(ex.ValidationFailures[0].ErrorMessage);
            }
        }

        /// <summary>
        /// Creates pre-defined system roles.
        /// </summary>
        /// <returns></returns>
        private async Task<IdentityResult> CreateSystemRolesAsync()
        {
            IdentityResult result = IdentityResult.Success;

            // Administrator role
            if (!await _roleManager.RoleExistsAsync(Role.ADMINISTRATOR_ROLE))
            {
                result = await _roleManager.CreateAsync(new Role
                {
                    Name = Role.ADMINISTRATOR_ROLE,
                    IsSystemRole = true,
                    Description = "Administrator has full power over the site and can do everything."
                });
                _logger.LogInformation($"{Role.ADMINISTRATOR_ROLE} role created.");
            }

            // Editor role
            if (!await _roleManager.RoleExistsAsync(Role.EDITOR_ROLE))
            {
                result = await _roleManager.CreateAsync(new Role
                {
                    Name = Role.EDITOR_ROLE,
                    IsSystemRole = true,
                    Description = "Editor can only publish and manage posts including the posts of other users."
                });
                _logger.LogInformation($"{Role.EDITOR_ROLE} role created.");
            }

            return result;
        }

        /// <summary>
        /// Creates the welcome blog post, settings, tags and the default category.
        /// </summary>
        /// <returns></returns>
        private async Task SetupBlogAsync()
        {
            const string DEFAULT_CATEGORY = "Uncategorized";
            const string WELCOME_POST_TITLE = "Welcome to Fanray v2.0";
            const string WELCOME_POST_BODY = @"<p>Welcome and congrats on getting your blog up and running! &nbsp;Fanray v1.0 was a minimal viable blog, you could only post through a client like the Open Live Writer. Fanray v2 builds on v1 and introduces a brand new Admin Panel, you can now do more! &nbsp;Here are a few tips to get you started and please check out the <a href=""https://github.com/FanrayMedia/Fanray/wiki"">Wiki</a> for more details.</p><h2>Getting Started</h2><p>First I recommend complete the setup by going to the <a href=""https://github.com/FanrayMedia/Fanray/wiki/Admin---Settings"">Settings</a> page and enter your Disqus and Google Analytics information.</p><p>Bloggers probably spend most of their time writing posts. To help you be productive, I highly recommend spending a few minutes to get familiar with the <a href=""https://github.com/FanrayMedia/Fanray/wiki/Admin---Composer"">Composer</a>, particularly the <a href=""https://github.com/FanrayMedia/Fanray/wiki/Admin---Composer#editor"">Editor</a>.</p><p>The <a href=""https://github.com/FanrayMedia/Fanray/wiki/Admin---Media-Gallery"">Media Gallery</a> gives you a grand view of all your blog images. &nbsp;Here you can upload more images or edit image info etc. The uploaded images are resized and stored on either the file system or Azure Blob Storage, you can configure which <a href=""https://github.com/FanrayMedia/Fanray/wiki/Storage"">Storage</a> in the <code>appsettings.json</code>.</p><p>The <a href=""https://github.com/FanrayMedia/Fanray/wiki/Admin---Categories"">Categories </a>and <a href=""https://github.com/FanrayMedia/Fanray/wiki/Admin---Tags"">Tags</a> pages allow you to easily manage your blog's classifications. &nbsp;For categories there is a default category out of box named <i>Uncategorized</i>, go rename it to something you write about.&nbsp;</p><p>When you are ready to run this app on Azure, please refer to <a href=""https://github.com/FanrayMedia/Fanray/wiki/Deploying-to-Azure"">Deploying to Azure</a>.</p><h2>Contribute</h2><p>Fanray is in its early stages and requires support to move ahead. You can contribute in many ways - ideas, bugs, testing and docs etc. please read the <a href=""https://github.com/FanrayMedia/Fanray/blob/master/CONTRIBUTING.md"">Contributing Guide</a>.&nbsp;</p><p>Finally, follow me <a href=""https://twitter.com/FanrayMedia"">@fanraymedia</a> and let me know what you think. Thank you and happy coding :)</p>";

            // create blog setting
            var blogSettings = await _settingSvc.GetSettingsAsync<BlogSettings>(); // could be initial or an existing blogsettings
            await _settingSvc.UpsertSettingsAsync(blogSettings);

            // get default cat
            Category defaultCat = null;
            try
            {
                defaultCat = await _catSvc.GetAsync(blogSettings.DefaultCategoryId);
            }
            catch (FanException)
            {
                defaultCat = await _catSvc.CreateAsync(DEFAULT_CATEGORY);
            }

            // TODO should I make create welcome post a option on setup
            // create welcome post and default category
            await _blogSvc.CreatePostAsync(new BlogPost
            {
                CategoryTitle = defaultCat.Title,
                TagTitles = new List<string> { "announcement", "blogging" },
                //Tags = await _tagSvc.GetTagsAsync(),
                Title = WELCOME_POST_TITLE,
                Body = WELCOME_POST_BODY,
                UserId = 1,
                Status = EPostStatus.Published,
                CommentStatus = ECommentStatus.AllowComments,
                CreatedOn = DateTimeOffset.Now,
            });
            _logger.LogInformation("Welcome post and default category created.");
            _logger.LogInformation("Blog Setup completes.");
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
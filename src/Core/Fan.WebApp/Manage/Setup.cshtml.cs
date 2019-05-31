using Fan.Blog.Enums;
using Fan.Blog.Models;
using Fan.Blog.Services.Interfaces;
using Fan.Exceptions;
using Fan.Membership;
using Fan.Plugins;
using Fan.Settings;
using Fan.Themes;
using Fan.WebApp.Widgets.SocialIcons;
using Fan.Widgets;
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

namespace Fan.WebApp.Manage
{
    public class SetupModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ISettingService _settingSvc;
        private readonly IThemeService _themeService;
        private readonly IBlogPostService _blogSvc;
        private readonly ICategoryService _catSvc;
        private readonly ITagService _tagSvc;
        private readonly IWidgetService _widgetSvc;
        private readonly IPluginService pluginService;
        private readonly ILogger<SetupModel> _logger;

        public SetupModel(
            UserManager<User> userManager,
            RoleManager<Role> roleManager,
            SignInManager<User> signInManager,
            IBlogPostService blogService,
            ICategoryService catService,
            ITagService tagService,
            ISettingService settingService,
            IThemeService themeService,
            IWidgetService widgetService,
            IPluginService pluginService,
            ILogger<SetupModel> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _blogSvc = blogService;
            _catSvc = catService;
            _tagSvc = tagService;
            _settingSvc = settingService;
            _themeService = themeService;
            _widgetSvc = widgetService;
            this.pluginService = pluginService;
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

                    // setup widgets
                    await SetupThemeAndWidgetsAsync();

                    // setup plugins
                    await SetupPluginsAsync();

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
            const string WELCOME_POST_TITLE = "Welcome to Fanray Blog";
            const string WELCOME_POST_BODY = @"<p>A few tips to get you started.&nbsp;</p><ul><li>Go to <a href=""/admin/settings"">Settings</a> and enter your Disqus and Google Analytics information.</li><li>Go to <a href=""/admin/widgets"">Widgets</a> and update Social Icons to your own media links.</li><li>Spend a few minutes to get familiar with the <a href=""https://github.com/FanrayMedia/Fanray/wiki/Admin---Composer"">Composer</a>, knowing how to effectively input different contents in the <a href=""https://github.com/FanrayMedia/Fanray/wiki/Admin---Composer#editor"">Editor</a> will help you become more productive blogging.</li><li>Check out <code>appsettings.json</code> to update database, storage, preferred domain, logging and diagnostic settings for local and production.</li><li>When you are ready to run this app on Azure, refer to <a href=""https://github.com/FanrayMedia/Fanray/wiki/Deploying-to-Azure"">Deploying to Azure</a>.</li><li>Feel like contributing? See the <a href=""https://github.com/FanrayMedia/Fanray/blob/master/CONTRIBUTING.md"">Contributing Guide</a>.</li></ul><p>Thank you and happy coding :)</p>";
            const string WELCOME_POST_EXCERPT = "Welcome to Fanray Blog. Here are a few tips to get you started using the blog.";

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
            await _blogSvc.CreateAsync(new BlogPost
            {
                CategoryTitle = defaultCat.Title,
                TagTitles = new List<string> { "announcement", "blogging" },
                Title = WELCOME_POST_TITLE,
                Body = WELCOME_POST_BODY,
                Excerpt = WELCOME_POST_EXCERPT,
                UserId = 1,
                Status = EPostStatus.Published,
                CommentStatus = ECommentStatus.AllowComments,
                CreatedOn = DateTimeOffset.Now,
            });
            _logger.LogInformation("Welcome post and default category created.");
            _logger.LogInformation("Blog Setup completes.");
        }

        /// <summary>
        /// Activiates the default Clarity theme, registers system-defined widget areas, 
        /// then load some widgets.
        /// </summary>
        private async Task SetupThemeAndWidgetsAsync()
        {
            // Clarity theme
            await _themeService.ActivateThemeAsync("Clarity");

            // System-defined Areas
            await _widgetSvc.RegisterAreaAsync(WidgetService.BlogSidebar1.Id);
            await _widgetSvc.RegisterAreaAsync(WidgetService.BlogSidebar2.Id);
            await _widgetSvc.RegisterAreaAsync(WidgetService.BlogBeforePost.Id);
            await _widgetSvc.RegisterAreaAsync(WidgetService.BlogAfterPost.Id);
            await _widgetSvc.RegisterAreaAsync(WidgetService.BlogBeforePostList.Id);
            await _widgetSvc.RegisterAreaAsync(WidgetService.BlogAfterPostList.Id);
            await _widgetSvc.RegisterAreaAsync(WidgetService.Footer1.Id);
            await _widgetSvc.RegisterAreaAsync(WidgetService.Footer2.Id);
            await _widgetSvc.RegisterAreaAsync(WidgetService.Footer3.Id);

            // Area: BlogSidebar1

            // Social Icons
            var socialIconsWidget = new SocialIconsWidget { Links = SocialIconsWidget.SocialLinkSeeds };
            var widgetInstId = await _widgetSvc.CreateWidgetAsync(socialIconsWidget, "SocialIcons");
            await _widgetSvc.AddWidgetToAreaAsync(widgetInstId, WidgetService.BlogSidebar1.Id, 0);

            // Blog Tags
            widgetInstId = await _widgetSvc.CreateWidgetAsync("BlogTags");
            await _widgetSvc.AddWidgetToAreaAsync(widgetInstId, WidgetService.BlogSidebar1.Id, 1);

            // Blog Categories
            widgetInstId = await _widgetSvc.CreateWidgetAsync("BlogCategories");
            await _widgetSvc.AddWidgetToAreaAsync(widgetInstId, WidgetService.BlogSidebar1.Id, 2);

            // Blog Archives
            widgetInstId = await _widgetSvc.CreateWidgetAsync("BlogArchives");
            await _widgetSvc.AddWidgetToAreaAsync(widgetInstId, WidgetService.BlogSidebar1.Id, 3);

            // Area: BlogAfterPost

            // Recent Blog Posts
            widgetInstId = await _widgetSvc.CreateWidgetAsync("RecentBlogPosts");
            await _widgetSvc.AddWidgetToAreaAsync(widgetInstId, WidgetService.BlogAfterPost.Id, 0);
        }

        /// <summary>
        /// Activates Shortcodes plugin.
        /// </summary>
        /// <returns></returns>
        private async Task SetupPluginsAsync()
        {
            await pluginService.ActivatePluginAsync("Shortcodes");
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

        public SetupValidator()
        {
            // Email
            RuleFor(s => s.Email).EmailAddress();

            // UserName
            RuleFor(s => s.UserName)
                .NotEmpty()
                .Length(NAME_MINLENGTH, USERNAME_MAXLENGTH)
                .Matches(USERNAME_REGEX)
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
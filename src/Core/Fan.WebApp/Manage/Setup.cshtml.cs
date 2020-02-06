using Fan.Blog.Enums;
using Fan.Blog.Models;
using Fan.Blog.Services.Interfaces;
using Fan.Exceptions;
using Fan.Membership;
using Fan.Navigation;
using Fan.Plugins;
using Fan.Settings;
using Fan.Themes;
using Fan.Widgets;
using FluentValidation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SocialIcons;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Fan.WebApp.Manage
{
    public class SetupModel : PageModel
    {
        private readonly IWebHostEnvironment hostingEnvironment;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ISettingService settingService;
        private readonly IThemeService _themeService;
        private readonly IPluginService pluginService;
        private readonly IBlogPostService _blogSvc;
        private readonly IPageService pageService;
        private readonly ICategoryService _catSvc;
        private readonly INavigationService navigationService;
        private readonly IWidgetService _widgetSvc;
        private readonly ILogger<SetupModel> _logger;

        public const string SETUP_DATA_DIR = "Setup";

        public SetupModel(
            IWebHostEnvironment hostingEnvironment,
            UserManager<User> userManager,
            RoleManager<Role> roleManager,
            SignInManager<User> signInManager,
            IBlogPostService blogService,
            IPageService pageService,
            ICategoryService catService,
            INavigationService navigationService,
            ISettingService settingService,
            IThemeService themeService,
            IPluginService pluginService,
            IWidgetService widgetService,
            ILogger<SetupModel> logger)
        {
            this.hostingEnvironment = hostingEnvironment;
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _blogSvc = blogService;
            this.pageService = pageService;
            _catSvc = catService;
            this.navigationService = navigationService;
            this.settingService = settingService;
            _themeService = themeService;
            this.pluginService = pluginService;
            _widgetSvc = widgetService;
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
            var coreSettings = await settingService.GetSettingsAsync<CoreSettings>();
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
                _logger.LogInformation("Fanray setup begins");

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
                    var settings = await settingService.GetSettingsAsync<CoreSettings>();
                    if (settings != null)
                    {
                        settings.Title = model.Title;
                        settings.TimeZoneId = model.TimeZoneId;
                        settings.SetupDone = true;
                        await settingService.UpsertSettingsAsync(settings);
                    }
                    else
                    {
                        await settingService.UpsertSettingsAsync(new CoreSettings
                        {
                            Title = model.Title,
                            TimeZoneId = model.TimeZoneId,
                            SetupDone = true,
                        });
                    }
                    _logger.LogInformation("CoreSettings created");

                    // sign-in user
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    _logger.LogInformation("User has been signed in");

                    // setup blog
                    await SetupBlogSettingsAndPostsAsync();
                    await SetupPagesAndNavigationAsync();
                    await SetupThemePluginsAndWidgetsAsync();
                    _logger.LogInformation("Blog setup completes");

                    return new JsonResult(true);
                }

                return BadRequest(result.Errors.ToList()[0].Description);
            }
            catch (FanException ex)
            {
                return BadRequest(ex.ValidationErrors[0].ErrorMessage);
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
        private async Task SetupBlogSettingsAndPostsAsync()
        {
            // -------------------------------------------- blog settings

            var blogSettings = await settingService.GetSettingsAsync<BlogSettings>(); // could be initial or an existing blogsettings
            await settingService.UpsertSettingsAsync(blogSettings);

            _logger.LogInformation("Blog settings created");

            // -------------------------------------------- categories

            const string DEFAULT_CATEGORY = "Software Development";
            const string SECOND_CATEGORY = "Life Musings";

            // get default cat
            Category defaultCat;
            try
            {
                defaultCat = await _catSvc.GetAsync(blogSettings.DefaultCategoryId);
            }
            catch (FanException)
            {
                defaultCat = await _catSvc.CreateAsync(DEFAULT_CATEGORY);
            }

            Category secondCat;
            try
            {
                secondCat = await _catSvc.GetAsync(blogSettings.DefaultCategoryId + 1);
            }
            catch (FanException)
            {
                secondCat = await _catSvc.CreateAsync(SECOND_CATEGORY);
            }

            _logger.LogInformation("Blog categories created");

            // -------------------------------------------- posts

            // Hello World (life musing)
            await _blogSvc.CreateAsync(new BlogPost
            {
                UserId = 1,
                Status = EPostStatus.Published,
                CommentStatus = ECommentStatus.AllowComments,
                CreatedOn = DateTimeOffset.Now,
                Title = "Hello World",
                Body = await GetSetupFileContent("post-hello.html"),
                Excerpt = "A blogging joke to break ice :)",
                CategoryTitle = secondCat.Title, 
                TagTitles = new List<string> { "blogging", "jokes" },
            });

            // Welcome to Fanray (software dev)
            await _blogSvc.CreateAsync(new BlogPost
            {
                UserId = 1,
                Status = EPostStatus.Published,
                CommentStatus = ECommentStatus.AllowComments,
                CreatedOn = DateTimeOffset.Now,
                Title = "Welcome to Fanray",
                Body = await GetSetupFileContent("post-welcome.html"),
                Excerpt = "Some tips to get you started blogging!",
                CategoryTitle = defaultCat.Title, 
                TagTitles = new List<string> { "blogging", "tips" },
            });

            _logger.LogInformation("Default posts created");
        }

        /// <summary>
        /// Creates default pages and setup navigation.
        /// </summary>
        /// <returns></returns>
        private async Task SetupPagesAndNavigationAsync()
        {
            // -------------------------------------------- pages

            // "docs" parent page
            var docsPage = await pageService.CreateAsync(new Blog.Models.Page
            {
                UserId = 1,
                Title = "Docs",
                Status = EPostStatus.Published,
                CreatedOn = DateTimeOffset.Now,
                PageLayout = (byte)EPageLayout.Layout3, // multi-page
                Body = await GetSetupFileContent("page-docs.html"),
                BodyMark = await GetSetupFileContent("page-docs.md"),
            });

            // "posts" child page
            await pageService.CreateAsync(new Blog.Models.Page
            {
                UserId = 1,
                ParentId = docsPage.Id,
                Title = "Posts",
                Status = EPostStatus.Published,
                CreatedOn = DateTimeOffset.Now,
                PageLayout = (byte)EPageLayout.Layout3, 
                Excerpt = "How to create posts using Fanray.",
                Body = await GetSetupFileContent("page-posts.html"),
                BodyMark = await GetSetupFileContent("page-posts.md"),
            });

            // "pages" child page
            await pageService.CreateAsync(new Blog.Models.Page
            {
                UserId = 1,
                ParentId = docsPage.Id,
                Title = "Pages",
                Status = EPostStatus.Published,
                CreatedOn = DateTimeOffset.Now,
                PageLayout = (byte)EPageLayout.Layout3,
                Excerpt = "How to create pages using Fanray.",
                Body = await GetSetupFileContent("page-pages.html"),
                BodyMark = await GetSetupFileContent("page-pages.md"),
            });

            // "add a home page" child page
            await pageService.CreateAsync(new Blog.Models.Page
            {
                UserId = 1,
                ParentId = docsPage.Id,
                Title = "Add a Home Page",
                Status = EPostStatus.Published,
                CreatedOn = DateTimeOffset.Now,
                PageLayout = (byte)EPageLayout.Layout3,
                Excerpt = "This exercise shows you how to add a Home page.",
                Body = await GetSetupFileContent("page-exercise.html"),
                BodyMark = await GetSetupFileContent("page-exercise.md"),
            });

            // page navigation
            await pageService.SaveNavAsync(docsPage.Id, await GetSetupFileContent("pagenav.md"));

            // "about"
            var aboutPage = await pageService.CreateAsync(new Blog.Models.Page
            {
                UserId = 1,
                Title = "About",
                Status = EPostStatus.Published,
                CreatedOn = DateTimeOffset.Now,
                PageLayout = (byte)EPageLayout.Layout1, // default
                Excerpt = "About the Fanray project.",
                Body = await GetSetupFileContent("page-about.html"),
                BodyMark = await GetSetupFileContent("page-about.md"),
            });

            _logger.LogInformation("Default pages created");

            // -------------------------------------------- site navigation

            // Blog (App)
            await navigationService.AddNavToMenuAsync(EMenu.Menu1, 0, new Nav
            {
                Id = App.BLOG_APP_ID,
                Text = App.BLOG_APP_NAME,
                Type = ENavType.App
            });

            // Docs (Page)
            await navigationService.AddNavToMenuAsync(EMenu.Menu1, 1, new Nav
            {
                Id = docsPage.Id,
                Text = docsPage.Title,
                Type = ENavType.Page
            });

            // About (Page)
            await navigationService.AddNavToMenuAsync(EMenu.Menu1, 2, new Nav
            {
                Id = aboutPage.Id,
                Text = aboutPage.Title,
                Type = ENavType.Page
            });

            _logger.LogInformation("Site navigation created");
        }

        /// <summary>
        /// Activiates the default Clarity theme and plugins and registers system-defined widget areas, 
        /// then load some widgets.
        /// </summary>
        private async Task SetupThemePluginsAndWidgetsAsync()
        {
            // Clarity theme
            await _themeService.ActivateThemeAsync("Clarity");

            // Activate plugins

            await pluginService.ActivatePluginAsync("Editor.md");
            await pluginService.ActivatePluginAsync("ForkMeRibbon");
            await pluginService.ActivatePluginAsync("Shortcodes");

            // System-defined Areas
            await _widgetSvc.RegisterAreaAsync(WidgetService.BlogSidebar1.Id);
            await _widgetSvc.RegisterAreaAsync(WidgetService.BlogSidebar2.Id);
            await _widgetSvc.RegisterAreaAsync(WidgetService.BlogBeforePost.Id);
            await _widgetSvc.RegisterAreaAsync(WidgetService.BlogAfterPost.Id);
            await _widgetSvc.RegisterAreaAsync(WidgetService.BlogBeforePostList.Id);
            await _widgetSvc.RegisterAreaAsync(WidgetService.BlogAfterPostList.Id);

            await _widgetSvc.RegisterAreaAsync(WidgetService.PageSidebar1.Id);
            await _widgetSvc.RegisterAreaAsync(WidgetService.PageSidebar2.Id);
            await _widgetSvc.RegisterAreaAsync(WidgetService.PageBeforeContent.Id);
            await _widgetSvc.RegisterAreaAsync(WidgetService.PageAfterContent.Id);

            await _widgetSvc.RegisterAreaAsync(WidgetService.Footer1.Id);
            await _widgetSvc.RegisterAreaAsync(WidgetService.Footer2.Id);
            await _widgetSvc.RegisterAreaAsync(WidgetService.Footer3.Id);

            // Area: BlogSidebar1

            // Social Icons
            var socialIconsWidget = new SocialIconsWidget { Links = SocialIconsWidget.SocialLinkSeeds };
            var socialIconsWidgetInstId = await _widgetSvc.CreateWidgetAsync(socialIconsWidget, "SocialIcons");
            await _widgetSvc.AddWidgetToAreaAsync(socialIconsWidgetInstId, WidgetService.BlogSidebar1.Id, 0);

            // Blog Tags
            var blogTagsWidgetInstId = await _widgetSvc.CreateWidgetAsync("BlogTags");
            await _widgetSvc.AddWidgetToAreaAsync(blogTagsWidgetInstId, WidgetService.BlogSidebar1.Id, 1);

            // Blog Categories
            var blogCatsWidgetInstId = await _widgetSvc.CreateWidgetAsync("BlogCategories");
            await _widgetSvc.AddWidgetToAreaAsync(blogCatsWidgetInstId, WidgetService.BlogSidebar1.Id, 2);

            // Blog Archives
            var blogArchivesWidgetInstId = await _widgetSvc.CreateWidgetAsync("BlogArchives");
            await _widgetSvc.AddWidgetToAreaAsync(blogArchivesWidgetInstId, WidgetService.BlogSidebar1.Id, 3);

            // Area: BlogAfterPost

            // Recent Blog Posts
            var recentBlogPostsWidgetInstId = await _widgetSvc.CreateWidgetAsync("RecentBlogPosts");
            await _widgetSvc.AddWidgetToAreaAsync(recentBlogPostsWidgetInstId, WidgetService.BlogAfterPost.Id, 0);

            // Area: PageSidebar1

            // Social Icons
            socialIconsWidgetInstId = await _widgetSvc.CreateWidgetAsync(socialIconsWidget, "SocialIcons");
            await _widgetSvc.AddWidgetToAreaAsync(socialIconsWidgetInstId, WidgetService.PageSidebar1.Id, 0);

            // Recent Blog Posts
            recentBlogPostsWidgetInstId = await _widgetSvc.CreateWidgetAsync("RecentBlogPosts");
            await _widgetSvc.AddWidgetToAreaAsync(recentBlogPostsWidgetInstId, WidgetService.PageSidebar1.Id, 1);

            // Area: PageSidebar2

            // Page Navigation
            var pageNavWidgetInstId = await _widgetSvc.CreateWidgetAsync("PageNavigation");
            await _widgetSvc.AddWidgetToAreaAsync(pageNavWidgetInstId, WidgetService.PageSidebar2.Id, 0);

            _logger.LogInformation("Theme and widgets created");
        }

        /// <summary>
        /// Returns the setup file content.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private async Task<string> GetSetupFileContent(string fileName)
        {
            var setupPath = Path.Combine(hostingEnvironment.ContentRootPath, SETUP_DATA_DIR);
            var filePath = Path.Combine(setupPath, fileName);
            return await System.IO.File.ReadAllTextAsync(filePath);
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
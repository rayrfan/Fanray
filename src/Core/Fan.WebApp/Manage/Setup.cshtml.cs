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
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SocialIcons;
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
        private readonly ISettingService settingService;
        private readonly IThemeService _themeService;
        private readonly IPluginService pluginService;
        private readonly IBlogPostService _blogSvc;
        private readonly IPageService pageService;
        private readonly ICategoryService _catSvc;
        private readonly INavigationService navigationService;
        private readonly IWidgetService _widgetSvc;
        private readonly ILogger<SetupModel> _logger;

        public SetupModel(
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

            await _blogSvc.CreateAsync(new BlogPost
            {
                UserId = 1,
                Status = EPostStatus.Published,
                CommentStatus = ECommentStatus.AllowComments,
                CreatedOn = DateTimeOffset.Now,
                Title = "Hello World",
                Body = @"<blockquote><p>As a blogger, my mind is always hazy, probably because I’m surrounded by so many drafts!</p></blockquote>",
                Excerpt = "A blogging joke to break ice :)",
                CategoryTitle = secondCat.Title, // life musings
                TagTitles = new List<string> { "blogging", "jokes" },
            });

            // TODO should I make create welcome post a option on setup
            // create welcome post and default category
            await _blogSvc.CreateAsync(new BlogPost
            {
                UserId = 1,
                Status = EPostStatus.Published,
                CommentStatus = ECommentStatus.AllowComments,
                CreatedOn = DateTimeOffset.Now,
                Title = "Welcome to Fanray",
                Body = @"<p>A few tips to get you started.&nbsp;</p><ul><li><a href=""/docs"">Docs</a> has guidelines that explain the basics of writing and editing posts and pages.</li><li>Go through the simple exercise <a href=""/docs/add-a-home-page"">Add a Home Page</a> to see how pages and site navigation work.</li><li>Go to <a href=""/admin/settings"">Settings</a> and enter your Disqus and Google Analytics information.</li><li>Go to <a href=""/admin/widgets"">Widgets</a> and update Social Icons to your own media links.</li><li>To see the 404 page in development when you go to a URL that does not exist, open <code>Fan.WebApp</code> project properties and set <code>ASPNETCORE_ENVIRONMENT</code> to <code>Production</code>.</li><li>Check out <code>appsettings.json</code> to update database, storage, preferred domain, logging and diagnostic settings for local and production.</li></ul>",
                Excerpt = "Some tips to get you started blogging!",
                CategoryTitle = defaultCat.Title, // software dev
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
                Body = @"<p><a href=""/docs/pages"" title=""Pages"">Pages</a>
 and <a href=""/docs/posts"" title=""Posts"">Posts</a>
 explain the basic things to be aware of when writing and editing with Fanray.</p>
<p>The <a href=""/docs/add-a-home-page"" title=""Add a Home Page"">Add a Home Page</a>
 exercise gives an example of how Pages and Site Navigation work.</p>
",
                BodyMark = @"[[Pages]] and [[Posts]] explain the basic things to be aware of when writing and editing with Fanray.

The [[Add a Home Page]] exercise gives an example of how Pages and Site Navigation work.",
            });

            // "posts" child page
            var postsPage = await pageService.CreateAsync(new Blog.Models.Page
            {
                UserId = 1,
                ParentId = docsPage.Id,
                Title = "Posts",
                Status = EPostStatus.Published,
                CreatedOn = DateTimeOffset.Now,
                PageLayout = (byte)EPageLayout.Layout3, 
                Excerpt = "How to create posts using Fanray.",
                Body = @"<div class=""markdown-toc editormd-markdown-toc""><ul class=""markdown-toc-list""><li><a class=""toc-level-2"" href=""#Introduction"" level=""2"">Introduction</a></li><li><a class=""toc-level-2"" href=""#Page Title and URL"" level=""2"">Page Title and URL</a></li><li><a class=""toc-level-2"" href=""#Editor"" level=""2"">Editor</a><ul><li><a class=""toc-level-3"" href=""#Keyboard Shortcuts"" level=""3"">Keyboard Shortcuts</a></li><li><a class=""toc-level-3"" href=""#Auto-save Draft"" level=""3"">Auto-save Draft</a><ul></ul></li></ul></li></ul></div><h2 id=""h2-introduction""><a name=""Introduction"" class=""reference-link""></a><span class=""header-link octicon octicon-link""></span>Introduction</h2><p>You can create posts using the built-in rich-text editor or a client like Open Live Writer.</p>
<h2 id=""h2-page-title-and-url""><a name=""Page Title and URL"" class=""reference-link""></a><span class=""header-link octicon octicon-link""></span>Page Title and URL</h2><p>A post’s slug is automatically derived from its title, or you can enter one manually. The post’s URL will be a combination of its post date and slug. Unlike <a href=""/docs/pages"" title=""pages"">pages</a>
, post titles do not have to be unique. If the derived URL happens to exist already, a number will be attached to the end of the URL.</p>
<h2 id=""h2-editor""><a name=""Editor"" class=""reference-link""></a><span class=""header-link octicon octicon-link""></span>Editor</h2><h3 id=""h3-keyboard-shortcuts""><a name=""Keyboard Shortcuts"" class=""reference-link""></a><span class=""header-link octicon octicon-link""></span>Keyboard Shortcuts</h3><table>
<thead>
<tr>
<th style=""text-align:left"">Shortcuts</th>
<th style=""text-align:left"">Description</th>
</tr>
</thead>
<tbody><tr>
<td style=""text-align:left""># + space</td>
<td style=""text-align:left"">Use one or more # for headings (h1, h2 etc.)</td>
</tr>
<tr>
<td style=""text-align:left"">&gt; + space</td>
<td style=""text-align:left"">Quote</td>
</tr>
<tr>
<td style=""text-align:left"">- + space</td>
<td style=""text-align:left"">Unordered list (ul)</td>
</tr>
<tr>
<td style=""text-align:left"">``</td>
<td style=""text-align:left"">Inline code</td>
</tr>
<tr>
<td style=""text-align:left"">Select text, Ctrl + k</td>
<td style=""text-align:left"">Make text link</td>
</tr>
<tr>
<td style=""text-align:left"">Shift + enter</td>
<td style=""text-align:left"">Soft break (br)</td>
</tr>
<tr>
<td style=""text-align:left"">Drag &amp; drop images</td>
<td style=""text-align:left"">Only png, jpg and gif files are supported</td>
</tr>
</tbody></table>
<h3 id=""h3-auto-save-draft""><a name=""Auto-save Draft"" class=""reference-link""></a><span class=""header-link octicon octicon-link""></span>Auto-save Draft</h3><p>Your work is auto-saved after typing is stopped for 10 seconds (default). It’s only for drafts. When you make changes to a published post, auto-save won’t save your work to prevent accidently showing your unfinished work to the public.</p>
",
                BodyMark = @"[TOC]

## Introduction

You can create posts using the built-in rich-text editor or a client like Open Live Writer.

## Page Title and URL

A post's slug is automatically derived from its title, or you can enter one manually. The post's URL will be a combination of its post date and slug. Unlike [[pages]], post titles do not have to be unique. If the derived URL happens to exist already, a number will be attached to the end of the URL.

## Editor

### Keyboard Shortcuts

| Shortcuts | Description |
| :-- | :-- |
| # + space | Use one or more # for headings (h1, h2 etc.) |
| > + space | Quote |
| - + space | Unordered list (ul) |
| ``  | Inline code |
| Select text, Ctrl + k | Make text link |
| Shift + enter | Soft break (br) |
| Drag & drop images | Only png, jpg and gif files are supported |

### Auto-save Draft

Your work is auto-saved after typing is stopped for 10 seconds (default). It's only for drafts. When you make changes to a published post, auto-save won't save your work to prevent accidently showing your unfinished work to the public.
",
            });

            // "pages" child page
            var pagesPage = await pageService.CreateAsync(new Blog.Models.Page
            {
                UserId = 1,
                ParentId = docsPage.Id,
                Title = "Pages",
                Status = EPostStatus.Published,
                CreatedOn = DateTimeOffset.Now,
                PageLayout = (byte)EPageLayout.Layout3,
                Excerpt = "How to create pages using Fanray.",
                Body = @"<div class=""markdown-toc editormd-markdown-toc""><ul class=""markdown-toc-list""><li><a class=""toc-level-2"" href=""#Introduction"" level=""2"">Introduction</a></li><li><a class=""toc-level-2"" href=""#Page Title and URL"" level=""2"">Page Title and URL</a></li><li><a class=""toc-level-2"" href=""#Editor"" level=""2"">Editor</a></li><li><a class=""toc-level-2"" href=""#Layouts"" level=""2"">Layouts</a></li><li><a class=""toc-level-2"" href=""#Page Navigation"" level=""2"">Page Navigation</a><ul></ul></li></ul></div><h2 id=""h2-introduction""><a name=""Introduction"" class=""reference-link""></a><span class=""header-link octicon octicon-link""></span>Introduction</h2><p>Fanray allows you to create simple pages using <strong>HTML</strong>, <strong>CSS</strong> and <strong>Markdown</strong>. Pages are hierarchical and two levels deep max, so a page is either a parent or a child. Only <em>published</em> pages are visible to the public; if a parent is a draft then none of its children will be visible to the public.</p>
<h2 id=""h2-page-title-and-url""><a name=""Page Title and URL"" class=""reference-link""></a><span class=""header-link octicon octicon-link""></span>Page Title and URL</h2><p>A page's URL is calculated from its title. Page URLs come right after the site root. The page title must be unique from its siblings.</p>
<h2 id=""h2-editor""><a name=""Editor"" class=""reference-link""></a><span class=""header-link octicon octicon-link""></span>Editor</h2><p>When the editor is in focus, type <code>Shift + Alt + H</code> will bring up a Help Dialog to show you the rest of the keyboard shortcuts the editor has. Images can be dragged and dropped directly. Auto-save draft works the same way like the post editor.</p>
<h2 id=""h2-layouts""><a name=""Layouts"" class=""reference-link""></a><span class=""header-link octicon octicon-link""></span>Layouts</h2><p>Theme designers can create different layouts for pages to suit different scenarios. For example, the default theme <strong>Clarity</strong> has 3 layouts. The layouts are defined in the theme's manifest file <code>theme.json</code>.</p>
<h2 id=""h2-page-navigation""><a name=""Page Navigation"" class=""reference-link""></a><span class=""header-link octicon octicon-link""></span>Page Navigation</h2><p>A parent page with at least one child will have the ability to create a page navigation, like the one shown on <strong>Docs</strong>. Make sure you have the Page Navigation <a href=""/admin/widgets"" title=""widget"">widget</a> in the according sidebar area.</p>
",
                BodyMark = @"[TOC]

## Introduction

Fanray allows you to create simple pages using **HTML**, **CSS** and **Markdown**. Pages are hierarchical and two levels deep max, so a page is either a parent or a child. Only *published* pages are visible to the public; if a parent is a draft then none of its children will be visible to the public.

## Page Title and URL

A page's URL is calculated from its title. Page URLs come right after the site root. The page title must be unique from its siblings.

## Editor

When the editor is in focus, type `Shift + Alt + H` will bring up a Help Dialog to show you the rest of the keyboard shortcuts the editor has. Images can be dragged and dropped directly. Auto-save draft works the same way like the post editor.

## Layouts

Theme designers can create different layouts for pages to suit different scenarios. For example, the default theme **Clarity** has 3 layouts. The layouts are defined in the theme&#39;s manifest file `theme.json`.

## Page Navigation

A parent page with at least one child will have the ability to create a page navigation, like the one shown on **Docs**. Make sure you have the Page Navigation [widget](/admin/widgets ""widget"") in the according sidebar area.
",
            });

            // "add a home page" child page
            var homePage = await pageService.CreateAsync(new Blog.Models.Page
            {
                UserId = 1,
                ParentId = docsPage.Id,
                Title = "Add a Home Page",
                Status = EPostStatus.Published,
                CreatedOn = DateTimeOffset.Now,
                PageLayout = (byte)EPageLayout.Layout3,
                Excerpt = "This exercise shows you how to add a Home page.",
                Body = @"<p>This exercise walks you through 3 simple steps to create a front page for your site.</p>
<div class=""markdown-toc editormd-markdown-toc""><ul class=""markdown-toc-list""><li><a class=""toc-level-2"" href=""#Create a New Page"" level=""2"">Create a New Page</a></li><li><a class=""toc-level-2"" href=""#Set “Home Page” as Home"" level=""2"">Set “Home Page” as Home</a></li><li><a class=""toc-level-2"" href=""#Add Link to Site Navigation"" level=""2"">Add Link to Site Navigation</a><ul></ul></li></ul></div><h2 id=""h2-create-a-new-page""><a name=""Create a New Page"" class=""reference-link""></a><span class=""header-link octicon octicon-link""></span>Create a New Page</h2><p>Go to <a href=""/admin/pages"">Pages</a> and add a new page.</p>
<ul>
<li><strong>Title</strong>: type in <code>Home</code></li><li><strong>Body</strong>: copy and paste in the following code</li></ul>
<pre><code class=""language-html"">&lt;style&gt;
.hero { text-align: center; margin-bottom: 50px; }
.hero h1 { font-size: 3rem }
.hero p { font-size: 1.6rem; line-height: 1.3; color: #6a8bad; }
&lt;/style&gt;
&lt;div class=""hero""&gt;
  &lt;h1&gt;Fanray&lt;/h1&gt;
  &lt;p&gt;A simple and elegant blog.&lt;/p&gt;
&lt;/div&gt;

&lt;style&gt;
.action { margin: 3rem 0 1rem 0; text-align: center }
&lt;/style&gt;
&lt;p class=""action""&gt;
  &lt;a href=""/blog"" class=""btn btn-primary btn-lg active"" role=""button"" aria-pressed=""true""&gt;Get Started →&lt;/a&gt;
&lt;/p&gt;</code></pre>
<ul>
<li><strong>Description</strong>: type in <code>Welcome to Fanray</code></li><li><strong>Layout</strong>: choose <code>Full</code> which gives you that full page design</li></ul>
<p>Click on <strong>Publish</strong> to make it public.</p>
<h2 id=""h2-set-home-page-as-home""><a name=""Set “Home Page” as Home"" class=""reference-link""></a><span class=""header-link octicon octicon-link""></span>Set “Home Page” as Home</h2><p>Go to <a href=""/admin/navigation"">Navigation</a>, you should see the new <strong>Home</strong> page listed under the <strong>Pages</strong> panel, click on the 3 vertical dots and you will see a <strong>Set as Home</strong> button, click on it. This will set the <strong>Home</strong> page as the root of your site.</p>
<h2 id=""h2-add-link-to-site-navigation""><a name=""Add Link to Site Navigation"" class=""reference-link""></a><span class=""header-link octicon octicon-link""></span>Add Link to Site Navigation</h2><p>Finally, you want to add a Home link on to the navigation. In the <a href=""/admin/navigation"">Navigation</a>, add a <strong>Custom Link</strong>.</p>
<ul>
<li><strong>URL</strong>: type in <code>/</code></li><li><strong>Text</strong>: type in <code>Home</code></li></ul>
<p>Click on <strong>Add To Menu</strong> to add it to the Main Menu. Then drag the <strong>Home</strong> item to the top as the first item on the navigation.</p>
",
                BodyMark = @"This exercise walks you through 3 simple steps to create a front page for your site.

[TOC]

## Create a New Page

Go to [Pages][1] and add a new page.

- **Title**: type in `Home`
- **Body**: copy and paste in the following code

```html
<style>
.hero { text-align: center; margin-bottom: 50px; }
.hero h1 { font-size: 3rem }
.hero p { font-size: 1.6rem; line-height: 1.3; color: #6a8bad; }
</style>
<div class=""hero"">
  <h1>Fanray</h1>
  <p>A simple and elegant blog.</p>
</div>

<style>
.action { margin: 3rem 0 1rem 0; text-align: center }
</style>
<p class=""action"">
  <a href=""/blog"" class=""btn btn-primary btn-lg active"" role=""button"" aria-pressed=""true"">Get Started →</a>
</p>
```

- **Description**: type in `Welcome to Fanray`
- **Layout**: choose `Full` which gives you that full page design

Click on **Publish** to make it public.

## Set ""Home Page"" as Home

Go to [Navigation][2], you should see the new **Home** page listed under the **Pages** panel, click on the 3 vertical dots and you will see a **Set as Home** button, click on it. This will set the **Home** page as the root of your site.

## Add Link to Site Navigation

Finally, you want to add a Home link on to the navigation. In the [Navigation][2], add a **Custom Link**.

- **URL**: type in `/`
- **Text**: type in `Home`

Click on **Add To Menu** to add it to the Main Menu. Then drag the **Home** item to the top as the first item on the navigation.

  [1]: /admin/pages
  [2]: /admin/navigation",
            });

            // page navigation
            await pageService.SaveNavAsync(docsPage.Id, @"# [[Docs]]

## Writing / Editing

- [[Pages]]
- [[Posts]]

## Exercise

- [[Add a Home Page]]
");

            // "about"
            var aboutPage = await pageService.CreateAsync(new Blog.Models.Page
            {
                UserId = 1,
                Title = "About",
                Status = EPostStatus.Published,
                CreatedOn = DateTimeOffset.Now,
                PageLayout = (byte)EPageLayout.Layout1, // default
                Excerpt = "About the Fanray project.",
                Body = @"<p>Fanray is a blog and web app starter kit for .NET Core. If this project is useful to you in any way, please consider support it. Your contribution will really help this project out! Thank you!</p>
<p><a href=""https://www.buymeacoffee.com/Fanray"" target=""_blank""><img src=""https://www.buymeacoffee.com/assets/img/custom_images/orange_img.png"" alt=""Buy Me A Coffee""></a> <a href=""https://paypal.me/FanrayMedia"" target=""_blank""><img src=""https://user-images.githubusercontent.com/633119/67153529-df349d80-f29f-11e9-979c-d84499723477.png"" alt=""PayPal Me"" width=""125"" height=""33"" style=""background-color: #5999db;border-radius: 6px;padding: 6px 20px""></a></p>
",
                BodyMark = @"Fanray is a blog and web app starter kit for .NET Core. If this project is useful to you in any way, please consider support it. Your contribution will really help this project out! Thank you!

<a href=""https://www.buymeacoffee.com/Fanray"" target=""_blank""><img src=""https://www.buymeacoffee.com/assets/img/custom_images/orange_img.png"" alt=""Buy Me A Coffee""></a>

<a href=""https://paypal.me/FanrayMedia"" target=""_blank""><img src=""https://user-images.githubusercontent.com/633119/67153529-df349d80-f29f-11e9-979c-d84499723477.png"" alt=""PayPal Me"" width=""125"" height=""33"" style=""background-color: #5999db;border-radius: 6px;padding: 6px 20px""></a>
",
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
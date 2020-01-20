using Fan.Blog.Enums;
using Fan.Blog.Services;
using Fan.Blog.Services.Interfaces;
using Fan.Exceptions;
using Fan.Membership;
using Fan.Navigation;
using Fan.Settings;
using Fan.Web.Attributes;
using Fan.Web.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Fan.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHomeHelper homeHelper;
        private readonly SignInManager<User> signInManager;
        private readonly IPageService pageService;
        private readonly ICategoryService categoryService;
        private readonly ISettingService settingService;
        private readonly IStatsService statsService;
        private readonly ILogger<HomeController> logger;

        public HomeController(
            IHomeHelper homeHelper,
            SignInManager<User> signInManager,
            IPageService pageService,
            ICategoryService categoryService,
            ISettingService settingService,
            IStatsService statsService,
            ILogger<HomeController> logger)
        {
            this.homeHelper = homeHelper;
            this.signInManager = signInManager;
            this.pageService = pageService;
            this.categoryService = categoryService;
            this.settingService = settingService;
            this.statsService = statsService;
            this.logger = logger;
        }

        /// <summary>
        /// Site home.
        /// </summary>
        /// <returns></returns>
        [ModelPreRender]
        public async Task<IActionResult> Index(int? page)
        {
            var coreSettings = await settingService.GetSettingsAsync<CoreSettings>();
            var nav = coreSettings.Home;

            if (nav.Type == ENavType.Page)
            {
                var page2 = await pageService.GetAsync(nav.Id);
                var (pagePath, pageModel) = await homeHelper.GetPageAsync(page2.Slug);
                await statsService.IncViewCountAsync(EPostType.Page, pageModel.Id);
                pageModel.ViewCount++;
                return View(pagePath, pageModel);
            }
            else if (nav.Type == ENavType.BlogCategory)
            {
                var cat = await categoryService.GetAsync(nav.Id);
                var (catPath, catModel) = await homeHelper.GetBlogCategoryAsync(cat.Slug, BlogPostService.DEFAULT_PAGE_INDEX);
                return View(catPath, catModel);
            }

            // default to blog app
            var (indexPath, indexModel) = await homeHelper.GetBlogIndexAsync(page);
            return View(indexPath, indexModel);
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
                return ((FanException)error).ExceptionType == EExceptionType.ResourceNotFound ? 
                    View("404") :
                    View("Error", error.Message);
            }

            // 500 or exception other than FanException occurred unhandled
            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            logger.LogInformation("User logged out.");
            return RedirectToAction(nameof(BlogController.Page), "Blog");
        }
    }
}

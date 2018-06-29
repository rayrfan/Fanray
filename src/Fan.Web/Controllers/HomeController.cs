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
using System.Collections.Generic;
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

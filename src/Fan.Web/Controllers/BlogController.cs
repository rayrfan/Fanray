using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Fan.Services;

namespace Fan.Web.Controllers
{
    public class BlogController : Controller
    {
        private readonly IBlogService _blogSvc;
        public BlogController(IBlogService blogService)
        {
            _blogSvc = blogService;
        }

        /// <summary>
        /// _Layout.html Link rel="EditURI" type="application/rsd+xml" title="RSD" href="~/rsd"
        /// </summary>
        /// <returns></returns>
        public IActionResult Rsd()
        {
            var rootUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}";
            return View("Rsd", rootUrl);
        }
    }
}
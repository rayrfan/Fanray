using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shortcodes.Controllers
{
    [Authorize(Roles = "Administrator")]
    public partial class ShortcodesController : Controller
    {
        public IActionResult Details()
        {
            return View();
        }
    }
}

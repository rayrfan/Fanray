using Fan.Models;
using Fan.Web.Models.AuthViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Fan.Web.Controllers
{
    /// <summary>
    /// Api for authentication.
    /// </summary>
    /// <remarks>
    /// This api is doing Cookie Authentication. With Identity after login is successful it outputs
    /// to the client a cookie named ".AspNetCore.Identity.Application".
    /// </remarks>
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public AuthController(UserManager<User> userManager,
            SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        /// <summary>
        /// User login.
        /// </summary>
        /// <param name="loginUser"></param>
        /// <remarks>
        /// Logout can still happen the traditional way.
        /// 
        /// TODO: 1. check role and return url; 2. error messages.
        /// With 2.1 !ModelState.IsValid is not necessary but error messages returned is buried deep.
        /// </remarks>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("[action]")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([FromBody] LoginVM loginUser)
        {
            bool isEmail;
            try
            {
                new MailAddress(loginUser.UserName);
                isEmail = true;
            }
            catch (FormatException)
            {
                isEmail = false;
            }

            var user = isEmail ? await _userManager.FindByEmailAsync(loginUser.UserName) :
                await _userManager.FindByNameAsync(loginUser.UserName);

            if (user == null)
                return BadRequest("Invalid credentials!");

            await _signInManager.SignInAsync(user, loginUser.RememberMe);

            return Ok();
        }

    }
}
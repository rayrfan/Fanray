using Fan.Membership;
using Fan.Web.Models.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
    [Route("api/auth")]
    [ApiController]
    public class AuthApiController : ControllerBase
    {
        private readonly IUserService _userSvc;
        private readonly SignInManager<User> _signInManager;

        public AuthApiController(IUserService userService,
            SignInManager<User> signInManager)
        {
            _userSvc = userService;
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
        public async Task<IActionResult> Login([FromBody] LoginViewModel loginUser)
        {
            // get user
            var user = await _userSvc.FindByEmailOrUsernameAsync(loginUser.UserName);
            if (user == null)
                return BadRequest("Invalid credentials!");

            // sign user in
            var result = await _signInManager.PasswordSignInAsync(user, loginUser.Password,
                loginUser.RememberMe, lockoutOnFailure: false);

            if (!result.Succeeded)
                return BadRequest("Invalid credentials!");

            return Ok();
        }
    }
}
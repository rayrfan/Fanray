using Fan.Membership;
using Fan.Settings;
using Fan.Web.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Fan.Web.ViewComponents
{
    public class HeaderViewComponent : ViewComponent
    {
        private readonly ISettingService _settingSvc;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public HeaderViewComponent(
            ISettingService settingService,
            UserManager<User> userManager,
            SignInManager<User> signInManager)
        {
            _settingSvc = settingService;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var coreSettings = await _settingSvc.GetSettingsAsync<CoreSettings>();
            var currentUser = await _userManager.GetUserAsync(Request.HttpContext.User);
            var isSignedIn = _signInManager.IsSignedIn(Request.HttpContext.User);

            var vm = new HeaderViewModel
            {
                Title = coreSettings.Title,
                Tagline = coreSettings.Tagline,
                CurrentUser = currentUser,
                IsSignedIn = isSignedIn,
            };

            return View(vm);
        }
    }
}

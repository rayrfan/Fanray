using Fan.Settings;
using Fan.Web.Mvc.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Fan.Web.Mvc.ViewComponents
{
    public class CookieConsentViewComponent : ViewComponent
    {
        private readonly HttpContext context;
        private readonly ISettingService _settingSvc;

        public CookieConsentViewComponent(
            IHttpContextAccessor contextAccessor,
            ISettingService settingService)
        {
            context = contextAccessor.HttpContext;
            _settingSvc = settingService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var coreSettings = await _settingSvc.GetSettingsAsync<CoreSettings>();
            var consentFeature = context.Features.Get<ITrackingConsentFeature>();
            var vm = new CookieConsentViewModel
            {
                ShowBanner = !consentFeature?.CanTrack ?? false,
                CookieString = consentFeature?.CreateConsentCookie(),
            };

            return View($"~/Themes/{coreSettings.Theme}/Views/Shared/CookieConsent.cshtml", vm);
        }
    }
}

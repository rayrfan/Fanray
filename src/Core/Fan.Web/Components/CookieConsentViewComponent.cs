using Fan.Settings;
using Fan.Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;

namespace Fan.Web.Components
{
    public class CookieConsentViewComponent : ViewComponent
    {
        private readonly HttpContext context;

        public CookieConsentViewComponent(
            IHttpContextAccessor contextAccessor,
            ISettingService settingService)
        {
            context = contextAccessor.HttpContext;
        }

        public IViewComponentResult Invoke()
        {
            var consentFeature = context.Features.Get<ITrackingConsentFeature>();
            var vm = new CookieConsentVM
            {
                ShowBanner = !consentFeature?.CanTrack ?? false,
                CookieString = consentFeature?.CreateConsentCookie(),
            };

            return View("CookieConsent.cshtml", vm);
        }
    }
}

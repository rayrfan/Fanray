using Fan.Web.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;

namespace Fan.Web.ViewComponents
{
    public class CookieConsentViewComponent : ViewComponent
    {
        private readonly HttpContext context;

        public CookieConsentViewComponent(IHttpContextAccessor contextAccessor)
        {
            context = contextAccessor.HttpContext;
        }

        public IViewComponentResult Invoke()
        {
            var consentFeature = context.Features.Get<ITrackingConsentFeature>();
            var vm = new CookieConsentViewModel
            {
                ShowBanner = !consentFeature?.CanTrack ?? false,
                CookieString = consentFeature?.CreateConsentCookie(),
            };

            return View(vm);
        }
    }
}

using Fan.Settings;
using Fan.Web.ViewModels;
using Microsoft.ApplicationInsights.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Fan.Web.ViewComponents
{
    /// <summary>
    /// The view component for Google Analytics and Application Insights.
    /// </summary>
    public class AnalyticsViewComponent : ViewComponent
    {
        private readonly ISettingService _settingSvc;
        private readonly JavaScriptSnippet _jsSnippet;
        private readonly IHostingEnvironment _env;

        public AnalyticsViewComponent(
            ISettingService settingService,
            JavaScriptSnippet jsSnippet,
            IHostingEnvironment env)
        {
            _settingSvc = settingService;
            _jsSnippet = jsSnippet;
            _env = env;
        }

        /// <summary>
        /// Returns Analytics/Default.csthml or nothing if neither GA nor AppIns is available.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Should I output this VC only when env is Production?
        /// </remarks>
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var coreSettings = await _settingSvc.GetSettingsAsync<CoreSettings>();
            var vm = new AnalyticsViewModel
            {
                GoogleAnalyticsTrackingID = coreSettings.GoogleAnalyticsTrackingID,
                AppInsightsFullScript = _jsSnippet.FullScript,
            };

            // if neither is available, show nothing
            if (vm.AppInsightsFullScript.IsNullOrEmpty() && vm.GoogleAnalyticsTrackingID.IsNullOrEmpty())
                return Content(string.Empty);

            return View($"~/Themes/{coreSettings.Theme}/Views/Shared/Analytics.cshtml", vm);
        }
    }
}

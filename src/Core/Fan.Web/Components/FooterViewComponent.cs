using Fan.Settings;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Fan.Web.Components
{
    public class FooterViewComponent : ViewComponent
    {
        private readonly ISettingService _settingSvc;

        public FooterViewComponent(ISettingService settingService)
        {
            _settingSvc = settingService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var coreSettings = await _settingSvc.GetSettingsAsync<CoreSettings>();
            return View("Footer.cshtml", coreSettings);
        }
    }
}

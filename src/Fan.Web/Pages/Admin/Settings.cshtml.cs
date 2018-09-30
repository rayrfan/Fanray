using Fan.Blog.Enums;
using Fan.Blog.Models;
using Fan.Blog.Services;
using Fan.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fan.Web.Pages.Admin
{
    public class SettingsModel : PageModel
    {
        private readonly ISettingService _settingSvc;
        private readonly IBlogService _blogSvc;

        public SettingsModel(
             IBlogService blogService,
            ISettingService settingService)
        {
            _blogSvc = blogService;
            _settingSvc = settingService;
        }

        public string TimeZoneId { get; set; }
        public List<SelectListItem> TimeZones { get; set; }
        public string TimeZonesJson { get; set; }
        public string PostListDisplaysJson { get; set; }

        public CoreSettings CoreSettings { get; private set; }
        public BlogSettings BlogSettings { get; private set; }

        /// <summary>
        /// GET bootstrap page with json data.
        /// </summary>
        /// <returns></returns>
        public async Task OnGetAsync()
        {
            CoreSettings = await _settingSvc.GetSettingsAsync<CoreSettings>();
            BlogSettings = await _settingSvc.GetSettingsAsync<BlogSettings>();

            TimeZones = new List<SelectListItem>();
            foreach (var tz in TimeZoneInfo.GetSystemTimeZones())
            {
                TimeZones.Add(new SelectListItem() { Value = tz.Id, Text = tz.DisplayName });
            }
            TimeZonesJson = JsonConvert.SerializeObject(TimeZones);

            var displayList = new List<string>();
            foreach (var display in Enum.GetValues(typeof(EPostListDisplay)))
            {
                displayList.Add(display.ToString());
            }
            PostListDisplaysJson = JsonConvert.SerializeObject(displayList);
        }

        /// <summary>
        /// Save site settings.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<IActionResult> OnPostSiteSettingsAsync([FromBody] CoreSettings model)
        {
            var settings = await _settingSvc.GetSettingsAsync<CoreSettings>();

            settings.Title = model.Title;
            settings.Tagline = model.Tagline;
            settings.TimeZoneId = model.TimeZoneId;
            settings.GoogleAnalyticsTrackingID = model.GoogleAnalyticsTrackingID;

            await _settingSvc.UpsertSettingsAsync(settings);
            return new JsonResult(true);
        }

        /// <summary>
        /// Save blog settings.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<IActionResult> OnPostBlogSettingsAsync([FromBody] BlogSettings model)
        {
            var settings = await _settingSvc.GetSettingsAsync<BlogSettings>();

            settings.PostListDisplay = model.PostListDisplay;
            settings.PostPerPage = model.PostPerPage;
            settings.DisqusShortname = model.DisqusShortname;
            settings.AllowComments = model.AllowComments;

            await _settingSvc.UpsertSettingsAsync(settings);
            return new JsonResult(true);
        }
    }
}
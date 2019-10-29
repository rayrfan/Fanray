using Fan.Blog.Enums;
using Fan.Blog.Models;
using Fan.Blog.Services.Interfaces;
using Fan.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fan.WebApp.Manage.Admin
{
    public class SettingsModel : PageModel
    {
        private readonly ISettingService settingService;
        private readonly IBlogPostService blogService;

        public SettingsModel(
             IBlogPostService blogService,
            ISettingService settingService)
        {
            this.blogService = blogService;
            this.settingService = settingService;
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
            CoreSettings = await settingService.GetSettingsAsync<CoreSettings>();
            BlogSettings = await settingService.GetSettingsAsync<BlogSettings>();

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
            var settings = await settingService.GetSettingsAsync<CoreSettings>();

            settings.Title = model.Title;
            settings.Tagline = model.Tagline;
            settings.TimeZoneId = model.TimeZoneId;
            settings.GoogleAnalyticsTrackingID = model.GoogleAnalyticsTrackingID;

            await settingService.UpsertSettingsAsync(settings);
            return new JsonResult(true);
        }

        /// <summary>
        /// Save blog settings.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<IActionResult> OnPostBlogSettingsAsync([FromBody] BlogSettings model)
        {
            var settings = await settingService.GetSettingsAsync<BlogSettings>();

            settings.PostListDisplay = model.PostListDisplay;
            settings.PostPerPage = model.PostPerPage;
            settings.DisqusShortname = model.DisqusShortname;
            settings.AllowComments = model.AllowComments;

            await settingService.UpsertSettingsAsync(settings);
            await blogService.RemoveBlogCacheAsync();
            return new JsonResult(true);
        }
    }
}
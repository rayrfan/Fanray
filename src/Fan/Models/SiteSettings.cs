using System;

namespace Fan.Models
{
    /// <summary>
    /// Settings for the over all site.
    /// </summary>
    public class SiteSettings
    {
        /// <summary>
        /// Title of the blog. Default "Fanray".
        /// </summary>
        public string Title { get; set; } = "Fanray";
        /// <summary>
        /// Short description on what the blog is about. Default "A fanray blog".
        /// </summary>
        public string Tagline { get; set; } = "A fanray blog";
        /// <summary>
        /// The Id of <see cref="TimeZoneInfo"/> class.
        /// </summary>
        /// <remarks>
        /// http://stackoverflow.com/a/7908482/32240
        /// </remarks>
        public string TimeZoneId { get; set; } = "Pacific Standard Time";

        /// <summary>
        /// Converts client local time to UTC time.
        /// </summary>
        public DateTime ToUtc(DateTime? localTime = null)
        {
            if (localTime == null || localTime == new DateTime())
                return DateTime.UtcNow;

            var userTimeZone = TimeZoneInfo.FindSystemTimeZoneById(string.IsNullOrEmpty(TimeZoneId) ? "UTC" : TimeZoneId);
            localTime = DateTime.SpecifyKind(localTime.Value, DateTimeKind.Unspecified);
            return TimeZoneInfo.ConvertTime(localTime.Value, userTimeZone, TimeZoneInfo.Utc);
        }

        /// <summary>
        /// Converts UTC from server into local client local time offset by timezone.
        /// </summary>
        public DateTime FromUtc(DateTime? serverTime = null)
        {
            if (serverTime == null || serverTime == new DateTime())
                serverTime = DateTime.UtcNow;

            var userTimeZone = TimeZoneInfo.FindSystemTimeZoneById(string.IsNullOrEmpty(TimeZoneId) ? "UTC" : TimeZoneId);
            serverTime = DateTime.SpecifyKind(serverTime.Value, DateTimeKind.Unspecified);
            return TimeZoneInfo.ConvertTime(serverTime.Value, TimeZoneInfo.Utc, userTimeZone);
        }
    }
}

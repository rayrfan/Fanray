using System;

namespace Fan.Models
{
    public class BlogSettings
    {
        /// <summary>
        /// Title of the blog. Default "Fanray".
        /// </summary>
        public string Title { get; set; } = "Fanray";
        /// <summary>
        /// The Id of <see cref="TimeZoneInfo"/> class.
        /// </summary>
        /// <remarks>
        /// http://stackoverflow.com/a/7908482/32240
        /// </remarks>
        public string TimeZoneId { get; set; } = "Pacific Standard Time";
        /// <summary>
        /// Number of blog posts to show. Default 10.
        /// </summary>
        public int PageSize { get; set; } = 10; 
        /// <summary>
        /// There must be one default category. Default 1.
        /// </summary>
        public int DefaultCategoryId { get; set; } = 1;
        /// <summary>
        /// How many words to extract into excerpt from body. Default 55.
        /// </summary>
        public int ExcerptWordLimit { get; set; } = 55;
        /// <summary>
        /// Should blog show a list of excerpt instead of body. Default false.
        /// </summary>
        public bool ShowExcerpt { get; set; } 

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

using Humanizer;
using TimeZoneConverter;

namespace System
{
    public static class DateTimeOffsetExtensions
    {
        /// <summary>
        /// Returns a humanized date time string if the <see cref="DateTimeOffset"/> falls within
        /// the given <paramref name="cutoffDays"/>, otherwise returns a formatted date time string.
        /// </summary>
        /// <param name="dt">The <see cref="DateTimeOffset"/>.</param>
        /// <param name="timeZoneId">User local timezone.</param>
        /// <param name="cutoffDays">Default 2.</param>
        /// <param name="format">Default "yyyy-MM-dd".</param>
        /// <returns></returns>
        public static string ToDisplayString(this DateTimeOffset dt, 
            string timeZoneId, 
            int cutoffDays = 2,
            string format = "yyyy-MM-dd")
        {
            return (DateTimeOffset.UtcNow.Day - dt.Day) > cutoffDays ?
                dt.ToLocalTime(timeZoneId).ToString(format) :
                dt.ToLocalTime(timeZoneId).Humanize();
        }

        /// <summary>
        /// Returns true if the DateTime is within a given number of days, false otherwise.
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="days"></param>
        /// <remarks>
        /// The result is an approximation, <see cref="DateTime"/> is used for simplicity.
        /// </remarks>
        public static bool IsWithinDays(this DateTime dt, int days)
        {
            var past = DateTime.UtcNow.AddDays(-days);
            return dt.CompareTo(past) >= 0;
        }

        /// <summary>
        /// Returns true if the year, month and day of the two <see cref="DateTimeOffset"/> are
        /// equal, false otherwise.
        /// </summary>
        /// <param name="dtSource"></param>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static bool YearMonthDayEquals(this DateTimeOffset dtSource, DateTimeOffset dt)
        {
            var source = new DateTimeOffset(dtSource.Year, dtSource.Month, dtSource.Day, 0, 0, 0, dtSource.Offset);
            var target = new DateTimeOffset(dt.Year, dt.Month, dt.Day, 0, 0, 0, dt.Offset);
            return source == target;
        }

        /// <summary>
        /// Converts a <see cref="DateTimeOffset"/> from the server to a user's local time with 
        /// his specific timezone.
        /// </summary>
        /// <param name="serverTime"></param>
        /// <param name="timeZoneId">The timezone to convert server time to.</param>
        /// <remarks>
        /// Posts are saved Utc time on server and are converted to <paramref name="timeZoneId"/>
        /// before displayed on the client.
        /// </remarks>
        public static DateTimeOffset ToLocalTime(this DateTimeOffset serverTime, string timeZoneId)
        {
            var userTimeZone = TZConvert.GetTimeZoneInfo(timeZoneId);
            return TimeZoneInfo.ConvertTime(serverTime, userTimeZone);
        }
    }
}

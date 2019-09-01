using Fan.Helpers;
using Humanizer;

namespace System
{
    public static class DateTimeOffsetExtensions
    {
        public static string ToDisplayString(this DateTimeOffset dt, string timeZoneId, string format)
        {
            return (DateTimeOffset.UtcNow.Day - dt.Day) > 2 ?
                Util.ConvertTime(dt, timeZoneId).ToString(format) :
                Util.ConvertTime(dt, timeZoneId).Humanize();
        }
    }
}

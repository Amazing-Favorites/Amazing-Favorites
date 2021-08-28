using System;

namespace Newbe.BookmarkManager.Services
{
    public static class TimeExtensions
    {
        public static DateTime ToLocalTime(this long time)
        {
            return DateTimeOffset.FromUnixTimeSeconds(time).DateTime.ToLocalTime();
        }

        public static DateTime? ToLocalTime(this long? time)
        {
            return time?.ToLocalTime();
        }
    }
}
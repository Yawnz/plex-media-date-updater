using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlexDateAddedUpdater.Helpers
{
    public static class DateTimeHelpers
    {
        public static int ToUnixEpoch(this DateTime dateTime)
        {
            TimeSpan t = dateTime - DateTime.UnixEpoch;

            return (int)t.TotalSeconds;
        }

        public static DateTime FromEpoch (int totalSeconds)
        {
            return DateTime.UnixEpoch.AddSeconds(totalSeconds);
        }
    }
}

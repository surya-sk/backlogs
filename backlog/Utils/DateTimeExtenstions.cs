using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backlogs.Utils
{
    public static class DateTimeExtenstions
    {
        public static DateTimeOffset ToDateTimeOffset(this DateTime dateTime)
        {
            return dateTime.ToUniversalTime() <= DateTimeOffset.MinValue.UtcDateTime
                       ? DateTimeOffset.MinValue
                       : new DateTimeOffset(dateTime);
        }
    }
}

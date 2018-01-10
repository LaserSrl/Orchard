using System;

namespace Laser.Orchard.Caligoo.Utils {
    public class CaligooUtils {
        private DateTime UNIX_TIME_ZERO = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        public int ConvertToTimestamp(DateTime dt) {
            return Convert.ToInt32((dt - UNIX_TIME_ZERO).TotalSeconds);
        }
    }
}
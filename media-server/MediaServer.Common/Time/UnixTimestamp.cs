using System;

namespace MediaServer.Common.Time
{
	public static class UnixTimestamp
	{
		public static double FromDateTime(DateTime dateTime)
		{
			return Math.Floor(
				dateTime.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds
				);
		}

        public static DateTime ToDateTime(long timestamp)
        {
			return new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).Add(TimeSpan.FromSeconds(timestamp));
		}
    }
}

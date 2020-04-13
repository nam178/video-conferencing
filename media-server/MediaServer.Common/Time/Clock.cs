using System;

namespace MediaServer.Common.Time
{
	sealed class Clock : IClock
	{
		public DateTime UtcNow => DateTime.UtcNow;
	}
}

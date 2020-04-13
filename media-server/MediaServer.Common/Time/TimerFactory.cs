using System;

namespace MediaServer.Common.Time
{
	sealed class TimerFactory : ITimerFactory
	{
		public ITimer Create(TimeSpan interval) => new TimerImpl(interval);
	}
}

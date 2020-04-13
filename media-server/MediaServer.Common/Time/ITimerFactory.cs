using System;

namespace MediaServer.Common.Time
{
	/// <summary>
	/// Creates a timer. We'll use this in the unit test project to create a fake timer.
	/// </summary>
	public interface ITimerFactory
	{
		ITimer Create(TimeSpan interval);
	}
}

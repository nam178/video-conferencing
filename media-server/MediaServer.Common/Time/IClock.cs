using System;

namespace MediaServer.Common.Time
{
	/// <summary>
	/// Interface around DateTime.UtcNow so time-dependent can be unit-tested easily.
	/// </summary>
	public interface IClock
	{
		DateTime UtcNow { get; }
	}
}

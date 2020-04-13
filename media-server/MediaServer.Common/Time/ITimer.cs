using System;

namespace MediaServer.Common.Time
{
	/// <summary>
	/// Basically a System.Timers.Timer, but we wrap around an interface,
	/// so components that depend on it can be unit tested.
	/// </summary>
	public interface ITimer : IDisposable
	{
		event EventHandler Ticked;

		bool AutoReset { get; set; }

		void Start();

		void Stop();
	}
}

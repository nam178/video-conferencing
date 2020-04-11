using System;

namespace MediaServer.WebRtc.Managed
{
    public sealed class EventArgs<T> : EventArgs
    {
        public T Value { get; }

        public EventArgs(T value)
        {
            Value = value;
        }
    }
}

using System;

namespace MediaServer.WebRtc.Managed
{
    sealed class EventArgs<T> : EventArgs
    {
        public T Value { get; }

        public EventArgs(T value)
        {
            Value = value;
        }
    }
}

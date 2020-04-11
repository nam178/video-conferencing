namespace MediaServer.Common.Utils
{
    public sealed class EventArgs<T> : System.EventArgs
    {
        public T Payload { get; }

        public EventArgs(T payload)
        {
            Payload = payload;
        }
    }
}

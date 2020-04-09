namespace MediaServer.Common.Utils
{
    sealed class EventArgs<T> : System.EventArgs
    {
        public T Target { get; }

        public EventArgs(T payload)
        {
            Target = payload;
        }
    }
}

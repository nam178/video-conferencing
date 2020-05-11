namespace MediaServer.Common.Patterns
{
    public interface IDispatcher<TEvent>
    {
        /// <summary>
        /// Asyncronously handle the specified event,
        /// immediately return to the caller as the caller doesn't care about the outcome.
        /// </summary>
        /// <param name="event"></param>
        void Dispatch(TEvent @event);
    }
}

using System;

namespace MediaServer.Common.Threading
{
    public interface IThread : IDispatchQueue
    {
        void Post(Action<object> handler, object userData = null);

        bool IsCurrent { get; }
    }

    public static class IThreadExtensions
    {
        public static void EnsureCurrentThread(this IThread thread)
        {
            if(!thread.IsCurrent)
            {
                throw new InvalidOperationException();
            }
        }
    }
}

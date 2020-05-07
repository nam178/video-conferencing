using System;

namespace MediaServer.Common.Threading
{
    public interface IThread : IDispatchQueue
    {
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

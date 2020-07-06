using System;
using System.Threading;

namespace MediaServer.Common.Patterns
{
    public class Disposer : IDisposable
    {
        readonly Action _impl;

        public Disposer(Action implementation)
        {
            _impl = implementation ?? throw new ArgumentNullException(nameof(implementation));
        }

        int _disposed;
        public void Dispose()
        {
            if(Interlocked.CompareExchange(ref _disposed, 1, 0) == 0)
            {
                _impl();
            }
        }
    }
}

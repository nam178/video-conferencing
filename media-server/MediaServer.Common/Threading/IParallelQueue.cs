using System;
using System.Diagnostics;

namespace MediaServer.Common.Threading
{
    public interface IParallelQueue
    {
        void Enqueue(object item, Action<object> task);
    }
}

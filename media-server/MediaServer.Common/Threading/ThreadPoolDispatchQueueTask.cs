using System.Threading.Tasks;

namespace MediaServer.Common.Threading
{
    abstract class ThreadPoolDispatchQueueTask
    {
        public abstract Task ExecuteAsync();
    }
}

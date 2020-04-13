using System;
using System.Threading.Tasks;

namespace MediaServer.Common.Threading
{
    sealed class ThreadPoolDispatchQueueRegisteredTask<TResult> : ThreadPoolDispatchQueueTask
    {
        readonly TaskCompletionSource<TResult> _src;
        readonly Func<Task<TResult>> _taskFactory;

        public ThreadPoolDispatchQueueRegisteredTask(TaskCompletionSource<TResult> src, Func<Task<TResult>> taskFactory)
        {
            _src = src;
            _taskFactory = taskFactory;
        }

        public override async Task ExecuteAsync()
        {
            TResult result;
            try
            {
                result = await _taskFactory();
            }
            catch(Exception ex)
            {
                Task.Run(() => _src.SetException(ex)).Forget();
                return;
            }

            Task.Run(() => _src.SetResult(result)).Forget();
        }
    }
}

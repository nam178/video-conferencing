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
            var originalTask = _taskFactory();

            try
            {
                var result  = await originalTask;
                _ = Task.Run(() => _src.SetResult(result));
            }
            catch(Exception ex)
            {
                _ = Task.Run(() => _src.SetException(ex));
            }
        }
    }
}

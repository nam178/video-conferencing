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

        public override Task ExecuteAsync()
        {
            var originalTask = _taskFactory();

            originalTask.ContinueWith(original =>
            {
                if(original.Status == TaskStatus.RanToCompletion)
                    _src.SetResult(original.Result);
                else if(original.Exception?.InnerException != null)
                    _src.SetException(original.Exception.InnerException);
                else
                    _src.SetCanceled();
            });

            return originalTask;
        }
    }
}

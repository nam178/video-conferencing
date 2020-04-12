using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace MediaServer.Common.Threading
{
    abstract class ThreadPoolDispatchQueueTask
    {
        public abstract Task ExecuteAsync();
    }

    sealed class RegisteredTask<TResult> : ThreadPoolDispatchQueueTask
    {
        readonly BlockingCollection<Action> _completedTasks;
        readonly TaskCompletionSource<TResult> _src;
        readonly Func<Task<TResult>> _taskFactory;

        public RegisteredTask(
            BlockingCollection<Action> completedTasks,
            TaskCompletionSource<TResult> src,
            Func<Task<TResult>> taskFactory)
        {
            _completedTasks = completedTasks;
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
                _completedTasks.Add(delegate
                {
                    _src.SetException(ex);
                });
                return;
            }

            _completedTasks.Add(delegate
            {
                _src.SetResult(result);
            });
        }
    }
}

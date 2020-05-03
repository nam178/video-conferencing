using NLog;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace MediaServer.Common.Threading
{
    public sealed class ThreadPoolDispatchQueue : IDispatchQueue, IDisposable
    {
        const int STATE_NOT_STARTED = 0;
        const int STATE_STARTED = 1;
        const int STATE_DISPOSED = 2;

        int _state = STATE_NOT_STARTED;

        readonly BlockingCollection<ThreadPoolDispatchQueueTask> _pendingTasks = new BlockingCollection<ThreadPoolDispatchQueueTask>();
        readonly CancellationTokenSource _cancelSource = new CancellationTokenSource();
        readonly static ILogger _logger = LogManager.GetCurrentClassLogger();
        readonly static AsyncLocal<bool> _workerThreadContext = new AsyncLocal<bool>();

        public ThreadPoolDispatchQueue(bool started = false)
        {
            if(started)
            {
                Start();
            }
        }

        public void RequireState(int expectedState)
        {
            if(Interlocked.CompareExchange(ref _state, expectedState, expectedState) != expectedState)
            {
                throw new InvalidOperationException();
            }
        }

        public Task ExecuteAsync(Action task)
        {
            if(task == null)
                throw new ArgumentNullException(nameof(task));
            RequireState(STATE_STARTED);

            // If we are in the context of worker thread,
            // Just execute the task immediately, no need to dispatch
            if(_workerThreadContext.Value)
            {
                task();
                return Task.CompletedTask;
            }

            // Queue this action to be executed in a later time
            var taskCompletionSource = new TaskCompletionSource<bool>();
            _pendingTasks.Add(new ThreadPoolDispatchQueueAction(taskCompletionSource, task));
            return taskCompletionSource.Task;
        }

        public Task ExecuteAsync(Func<Task> asyncTask)
        {
            if(asyncTask == null)
                throw new ArgumentNullException(nameof(asyncTask));
            RequireState(STATE_STARTED);

            // If we are in the context of worker thread,
            // Just execute the task immediately, no need to dispatch
            if(_workerThreadContext.Value)
            {
                return asyncTask();
            }

            // Queue this task to be executed at a later time
            var src = new TaskCompletionSource<bool>();
            _pendingTasks.Add(new ThreadPoolDispatchQueueRegisteredTask<bool>(src, async delegate
            {
                await asyncTask();
                return true;
            }));
            return src.Task;
        }

        public Task<TResult> ExecuteAsync<TResult>(Func<Task<TResult>> asyncTaskWithResult)
        {
            if(asyncTaskWithResult == null)
                throw new ArgumentNullException(nameof(asyncTaskWithResult));
            RequireState(STATE_STARTED);

            // If we are in the context of worker thread,
            // Just execute the task immediately, no need to dispatch
            if(_workerThreadContext.Value)
            {
                return asyncTaskWithResult();
            }

            // Queue this task to be executed at a later time
            var src = new TaskCompletionSource<TResult>();
            _pendingTasks.Add(new ThreadPoolDispatchQueueRegisteredTask<TResult>(src, asyncTaskWithResult));
            return src.Task;
        }

        public Task<TResult> ExecuteAsync<TResult>(Func<TResult> actionWithResult)
        {
            if(actionWithResult == null)
                throw new ArgumentNullException(nameof(actionWithResult));
            RequireState(STATE_STARTED);

            // If we are in the context of worker thread,
            // Just execute the task immediately, no need to dispatch
            if(_workerThreadContext.Value)
            {
                return Task.FromResult(actionWithResult());
            }

            // Queue this task to be executed at a later time
            var src = new TaskCompletionSource<TResult>();
            _pendingTasks.Add(
                new ThreadPoolDispatchQueueRegisteredTask<TResult>(
                    src,
                    () => Task.FromResult(actionWithResult())));
            return src.Task;
        }

        public void Dispose()
        {
            Interlocked.Exchange(ref _state, STATE_DISPOSED);
            try
            {
                _cancelSource.Cancel();
            }
            catch { }
            try
            {
                _pendingTasks.CompleteAdding();
            }
            catch { }
        }

        public void Start()
        {
            if(Interlocked.CompareExchange(ref _state, STATE_STARTED, STATE_NOT_STARTED) != STATE_NOT_STARTED)
            {
                throw new InvalidOperationException();
            }
            Task.Run(TaskExecutionThread).Mute<OperationCanceledException>().Forget();
        }

        async Task TaskExecutionThread()
        {
            _workerThreadContext.Value = true;

            using(_pendingTasks)
            using(_cancelSource)
            {
                foreach(var task in _pendingTasks.GetConsumingEnumerable(_cancelSource.Token))
                {
                    try
                    {
                        await task.ExecuteAsync();
                    }
                    catch(Exception ex) { _logger.Error(ex); }
                }
            }
        }
    }
}

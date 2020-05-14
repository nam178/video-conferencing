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
        readonly static AsyncLocal<Guid> _asyncLocalQueueId = new AsyncLocal<Guid>();
        readonly Guid _queueId = Guid.NewGuid();

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

            // This will cause dead lock, 
            // a task executing in this queue trying to wait for 
            // another task also scheduled into this queue.
            if(_asyncLocalQueueId.Value == _queueId)
            {
                throw new InvalidProgramException();
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

            // This will cause dead lock, 
            // a task executing in this queue trying to wait for 
            // another task also scheduled into this queue.
            if(_asyncLocalQueueId.Value == _queueId)
            {
                throw new InvalidProgramException();
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

            // This will cause dead lock, 
            // a task executing in this queue trying to wait for 
            // another task also scheduled into this queue.
            if(_asyncLocalQueueId.Value == _queueId)
            {
                throw new InvalidProgramException();
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

            // This will cause dead lock, 
            // a task executing in this queue trying to wait for 
            // another task also scheduled into this queue.
            if(_asyncLocalQueueId.Value == _queueId)
            {
                throw new InvalidProgramException();
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
            Task.Factory
                .StartNew(TaskExecutionThread, TaskCreationOptions.LongRunning)
                .ContinueWith(task =>
                {
                    var ex = task.Exception?.InnerException;
                    if(ex != null && !(ex is OperationCanceledException))
                    {
                        _logger.Error(ex);
                    }
                });
        }

        async Task TaskExecutionThread()
        {
            _asyncLocalQueueId.Value = _queueId;

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

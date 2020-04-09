using Autofac;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace MediaServer.Common.Threading
{
    public sealed class DispatchQueue : IDispatchQueue, IDisposable
    {
        const int MaxPendingTasks = 16;
        const int MaxPendingNotifications = 16;

        readonly BlockingCollection<RegisteredTask> _pendingTasks = new BlockingCollection<RegisteredTask>(MaxPendingTasks);
        readonly BlockingCollection<Action> _completedTasks = new BlockingCollection<Action>(MaxPendingNotifications);

        readonly static ILogger _logger = LogManager.GetCurrentClassLogger();
        readonly static AsyncLocal<bool> _workerThreadContext = new AsyncLocal<bool>();

        abstract class RegisteredTask
        {
            public abstract Task ExecuteAsync();
        }

        sealed class RegisteredAction : RegisteredTask
        {
            readonly TaskCompletionSource<bool> _src;
            readonly Action _action;

            public RegisteredAction(
                TaskCompletionSource<bool> src,
                Action action)
            {
                _src = src;
                _action = action;
            }

            public override Task ExecuteAsync()
            {
                try
                {
                    _action();
                }
                catch(Exception ex)
                {
                    _logger.Error(ex);
                    _src.SetException(ex);
                    return Task.CompletedTask;
                }

                _src.SetResult(true);
                return Task.CompletedTask;
            }
        }

        sealed class RegisteredTask<TResult> : RegisteredTask
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

        public Task ExecuteAsync(Action task)
        {
            if(task == null)
                throw new ArgumentNullException(nameof(task));

            // If we are in the context of worker thread,
            // Just execute the task immediately, no need to dispatch
            if(_workerThreadContext.Value)
            {
                task();
                return Task.CompletedTask;
            }

            // Queue this action to be executed in a later time
            var taskCompletionSource = new TaskCompletionSource<bool>();
            _pendingTasks.Add(new RegisteredAction(taskCompletionSource, task));
            return taskCompletionSource.Task;
        }

        public Task ExecuteAsync(Func<Task> asyncTask)
        {
            if(asyncTask == null)
                throw new ArgumentNullException(nameof(asyncTask));

            // If we are in the context of worker thread,
            // Just execute the task immediately, no need to dispatch
            if(_workerThreadContext.Value)
            {
                return asyncTask();
            }

            // Queue this task to be executed at a later time
            var src = new TaskCompletionSource<bool>();
            _pendingTasks.Add(new RegisteredTask<bool>(_completedTasks, src, async delegate
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

            // If we are in the context of worker thread,
            // Just execute the task immediately, no need to dispatch
            if(_workerThreadContext.Value)
            {
                return asyncTaskWithResult();
            }

            // Queue this task to be executed at a later time
            var src = new TaskCompletionSource<TResult>();
            _pendingTasks.Add(new RegisteredTask<TResult>(_completedTasks, src, asyncTaskWithResult));
            return src.Task;
        }

        public Task<TResult> ExecuteAsync<TResult>(Func<TResult> actionWithResult)
        {
            if(actionWithResult == null)
                throw new ArgumentNullException(nameof(actionWithResult));

            // If we are in the context of worker thread,
            // Just execute the task immediately, no need to dispatch
            if(_workerThreadContext.Value)
            {
                return Task.FromResult(actionWithResult());
            }

            // Queue this task to be executed at a later time
            var src = new TaskCompletionSource<TResult>();
            _pendingTasks.Add(
                new RegisteredTask<TResult>(
                    _completedTasks,
                    src,
                    () => Task.FromResult(actionWithResult())));
            return src.Task;
        }

        public void Dispose()
        {
            try
            {
                _pendingTasks.Dispose();
            }
            catch { }
            try
            {
                _completedTasks.Dispose();
            }
            catch { }
        }

        public void Start()
        {
            Task.Run(TaskExecutionThread);
            Task.Run(TaskCompletionNotificationThread);
        }

        async Task TaskExecutionThread()
        {
            _workerThreadContext.Value = true;

            foreach(var task in _pendingTasks.GetConsumingEnumerable())
            {

                try
                {
                    await task.ExecuteAsync();
                }
                catch(Exception ex) { _logger.Error(ex); }
            }
        }

        void TaskCompletionNotificationThread()
        {
            foreach(var completedTask in _completedTasks.GetConsumingEnumerable())
            {
                try
                {
                    completedTask();
                }
                catch(Exception ex) { _logger.Error(ex); }

            }
        }

        public static DispatchQueue CentralDispatchQueue { get; } = new DispatchQueue();
    }
}

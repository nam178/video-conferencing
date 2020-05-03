using MediaServer.Common.Threading;
using System;
using System.Threading.Tasks;

namespace MediaServer.WebRtc.Managed
{
    public sealed class RtcThread2DispatchQueueAdapter : IDispatchQueue
    {
        readonly RtcThread _original;

        public RtcThread2DispatchQueueAdapter(RtcThread original)
        {
            _original = original ?? throw new ArgumentNullException(nameof(original));
        }

        public Task ExecuteAsync(Action task)
        {
            var taskCompletionSource = new TaskCompletionSource<bool>();

            _original.Post(userData =>
            {
                try
                {
                    task();
                    Task.Run(() => taskCompletionSource.SetResult(true));
                }
                catch(Exception ex)
                {
                    Task.Run(() => taskCompletionSource.SetException(ex));
                }
            }, null);

            return taskCompletionSource.Task;
        }

        // notes
        // C++ can't C# tasks, therefore these methods left unimplemented.

        public Task ExecuteAsync(Func<Task> asyncTask) => throw new NotSupportedException();

        public Task<TResult> ExecuteAsync<TResult>(Func<Task<TResult>> asyncTaskWithResult) => throw new NotSupportedException();

        public Task<TResult> ExecuteAsync<TResult>(Func<TResult> actionWithResult)
        {
            var taskCompletionSource = new TaskCompletionSource<TResult>();

            _original.Post(userData =>
            {
                try
                {
                    var result = actionWithResult();
                    Task.Run(() => taskCompletionSource.SetResult(result));
                }
                catch(Exception ex)
                {
                    Task.Run(() => taskCompletionSource.SetException(ex));
                }
            }, null);

            return taskCompletionSource.Task;
        }
    }
}

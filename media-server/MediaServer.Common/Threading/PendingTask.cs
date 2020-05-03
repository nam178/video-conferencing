using System;
using System.Threading.Tasks;

namespace MediaServer.Common.Threading
{
    public sealed class PendingTask<T> : IPendingTask
    {
        public TaskCompletionSource<T> Source { get; }

        public Task<T> Task => Source.Task;

        public PendingTask()
        {
            Source = new TaskCompletionSource<T>();
        }

        public PendingTask(TaskCompletionSource<T> source)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
        }

        public void Cancel()
        {
            System.Threading.Tasks.Task.Run(delegate
            {
                Source.TrySetException(new TaskCanceledException());
            });
        }

        public static implicit operator PendingTask<T>(TaskCompletionSource<T> x) => new PendingTask<T>(x);
        public static implicit operator TaskCompletionSource<T>(PendingTask<T> y) => y.Source;
    }
}

using System;
using System.Threading.Tasks;

namespace MediaServer.Common.Threading
{
    public interface IDispatchQueue
    {
        Task ExecuteAsync(Action task);

        Task ExecuteAsync(Func<Task> asyncTask);

        Task<TResult> ExecuteAsync<TResult>(Func<Task<TResult>> asyncTaskWithResult);

        Task<TResult> ExecuteAsync<TResult>(Func<TResult> actionWithResult);
    }
}

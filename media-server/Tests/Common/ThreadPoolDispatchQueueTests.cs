using MediaServer.Common.Threading;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Tests.Common
{
    public class ThreadPoolDispatchQueueTests
    {
        [Fact]
        public void ExecuteAsync_ProvidedSyncrionousTask_TaskWillGetExecuted()
        {
            using var taskQueue = new ThreadPoolDispatchQueue();
            using var manualResetEvent = new ManualResetEvent(false);

            taskQueue.Start();

            // Execute a task
            var task = taskQueue.ExecuteAsync(delegate
            {
                // pretend executing
                Task.Delay(TimeSpan.FromSeconds(1)).Wait();
            });

            // Once the task is completed, trigger us
            task.ContinueWith(t =>
            {
                manualResetEvent.Set();
            });

            // Wait max 60 seconds
            // The task above must be completed within this provided time
            Assert.True(manualResetEvent.WaitOne(TimeSpan.FromSeconds(60)));
            Assert.True(task.IsCompleted);
        }

        [Fact]
        public async Task ExecuteAsync_ProvidedTaskThrowsException_ExceptionBubblesUp()
        {
            using var taskQueue = new ThreadPoolDispatchQueue();
            using var manualResetEvent = new ManualResetEvent(false);

            taskQueue.Start();

            await Assert.ThrowsAsync<DataMisalignedException>(async delegate
            {
                await taskQueue.ExecuteAsync(delegate
                {
                    throw new System.DataMisalignedException();
                });
            });
        }

        [Fact]
        public async Task ExecuteAsync_QueueNotStarted_ThrowsException()
        {
            using var taskQueue = new ThreadPoolDispatchQueue();
            using var manualResetEvent = new ManualResetEvent(false);

            await Assert.ThrowsAsync<InvalidOperationException>(async delegate
            {
                await taskQueue.ExecuteAsync(delegate { });
            });
            await Assert.ThrowsAsync<InvalidOperationException>(async delegate
            {
                await taskQueue.ExecuteAsync(async delegate { await Task.CompletedTask; });
            });
        }
    }
}

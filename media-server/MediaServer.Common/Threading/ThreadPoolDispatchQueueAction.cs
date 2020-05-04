using NLog;
using System;
using System.Threading.Tasks;

namespace MediaServer.Common.Threading
{
    sealed class ThreadPoolDispatchQueueAction : ThreadPoolDispatchQueueTask
    {
        readonly TaskCompletionSource<bool> _src;
        readonly Action _action;
        readonly static ILogger _logger = NLog.LogManager.GetCurrentClassLogger();

        public ThreadPoolDispatchQueueAction(
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
                Task.Run(() => _src.SetException(ex));
                return Task.CompletedTask;
            }

            Task.Run(() => _src.SetResult(true));
            return Task.CompletedTask;
        }
    }
}

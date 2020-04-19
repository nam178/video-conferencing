using NLog;
using System;
using System.Threading.Tasks;

namespace MediaServer.Common.Threading
{
    public static class TaskExtensions
    {
        static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        public static async Task Mute<TException>(this Task task) where TException :Exception
        {
            try
            {
                await task;
            }
            catch(TException) { }
        }

        public static async void Forget(this Task task, string errorMessage = null)
        {
            try
            {
                await task;
            }
            catch(Exception ex)
            {
                _logger.Warn(ex, errorMessage ?? "Error caught when fire & forget task");
            }
        }
    }
}

using NLog;
using System;
using System.Threading.Tasks;

namespace MediaServer.Common.Threading
{
    public static class TaskExtensions
    {
        static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        public static async void Forget(this Task task)
        {
            try
            {
                await task;
            }
            catch(Exception ex)
            {
                _logger.Error(ex, "Error caught when fire & forget task");
            }
        }
    }
}

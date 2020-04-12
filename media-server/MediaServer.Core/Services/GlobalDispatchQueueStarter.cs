using MediaServer.Common.Threading;
using Microsoft.Extensions.Hosting;
using NLog;
using System.Threading;
using System.Threading.Tasks;

namespace MediaServer.Core.Services
{
    sealed class GlobalDispatchQueueStarter : IHostedService
    {
        public ThreadPoolDispatchQueue DispatchQueue { get; }

        readonly ILogger _logger = NLog.LogManager.GetCurrentClassLogger();

        public GlobalDispatchQueueStarter(ThreadPoolDispatchQueue globalThreadPoolDispatchQueue)
        {
            DispatchQueue = globalThreadPoolDispatchQueue 
                ?? throw new System.ArgumentNullException(nameof(globalThreadPoolDispatchQueue));
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            DispatchQueue.Start();
            _logger.Info($"Global dispatch queue started");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}

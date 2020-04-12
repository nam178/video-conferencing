using MediaServer.Common.Threading;
using Microsoft.Extensions.Hosting;
using NLog;
using System.Threading;
using System.Threading.Tasks;

namespace MediaServer.Core.Services
{
    sealed class CentralDispatchQueueStarter : IHostedService
    {
        public ThreadPoolDispatchQueue DispatchQueue { get; }

        readonly ILogger _logger = NLog.LogManager.GetCurrentClassLogger();

        public CentralDispatchQueueStarter(ThreadPoolDispatchQueue centralDispatchQueue)
        {
            DispatchQueue = centralDispatchQueue 
                ?? throw new System.ArgumentNullException(nameof(centralDispatchQueue));
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

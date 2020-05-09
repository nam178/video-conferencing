using Microsoft.Extensions.Hosting;
using NLog;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace MediaServer
{
    sealed class EnterNewLineService : IHostedService
    {
        readonly static ILogger _logger = LogManager.GetCurrentClassLogger();

        public Task StartAsync(CancellationToken cancellationToken)
        {
            if(Debugger.IsAttached)
            {
                Task.Run(delegate
                {
                    while(true)
                    {
                        var line = Console.ReadLine();
                        _logger.Trace(String.Empty);
                    }
                });
            }
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}

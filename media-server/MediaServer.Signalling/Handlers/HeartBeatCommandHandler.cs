using MediaServer.Common.Commands;
using MediaServer.Signalling.CommandArgs;
using MediaServer.Signalling.Net;
using NLog;
using System.Threading.Tasks;

namespace MediaServer.Signalling.Handlers
{
    sealed class HeartBeatCommandHandler : ICommandHandler<WebSocketClient, HeartBeat>
    {
        static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        public Task HandleAsync(WebSocketClient arg1, HeartBeat arg2) => Task.CompletedTask;
    }
}

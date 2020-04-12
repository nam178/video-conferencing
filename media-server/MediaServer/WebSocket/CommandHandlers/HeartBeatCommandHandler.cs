using MediaServer.Common.Commands;
using MediaServer.WebSocket.CommandArgs;
using NLog;
using System.Threading.Tasks;

namespace MediaServer.WebSocket.CommandHandlers
{
    sealed class HeartBeatCommandHandler : ICommandHandler<WebSocketClient, HeartBeat>
    {
        static readonly ILogger _logger = NLog.LogManager.GetCurrentClassLogger();

        public Task HandleAsync(WebSocketClient arg1, HeartBeat arg2) => Task.CompletedTask;
    }
}

using MediaServer.Common.Mediator;
using MediaServer.WebSocket.Net;
using NLog;
using System.Threading.Tasks;

namespace MediaServer.WebSocket.CommandHandlers
{
    sealed class HeartBeatCommandHandler : IHandler<IWebSocketRemoteDevice, CommandArgs.HeartBeat>
    {
        readonly static ILogger _logger = NLog.LogManager.GetCurrentClassLogger();

        public Task HandleAsync(IWebSocketRemoteDevice device, CommandArgs.HeartBeat arg2)
        {
            _logger.Trace($"{device} heart beat received");
            return Task.CompletedTask;
        }
    }
}

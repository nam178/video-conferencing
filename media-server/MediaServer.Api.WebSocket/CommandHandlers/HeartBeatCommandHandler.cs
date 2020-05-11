using MediaServer.Common.Time;
using MediaServer.Api.WebSocket.Net;
using NLog;
using System.Threading.Tasks;
using MediaServer.Common.Patterns;

namespace MediaServer.Api.WebSocket.CommandHandlers
{
    sealed class HeartBeatCommandHandler : IHandler<IWebSocketRemoteDevice, CommandArgs.HeartBeat>
    {
        readonly static ILogger _logger = NLog.LogManager.GetCurrentClassLogger();

        public Task HandleAsync(IWebSocketRemoteDevice device, CommandArgs.HeartBeat arg2)
        {
            // _logger.Trace($"{device} heart beat received, client timestamp={UnixTimestamp.ToDateTime(arg2.Timestamp)}");
            return Task.CompletedTask;
        }
    }
}

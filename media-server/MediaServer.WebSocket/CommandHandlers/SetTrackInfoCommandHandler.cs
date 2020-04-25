using MediaServer.Common.Mediator;
using MediaServer.WebSocket.CommandArgs;
using MediaServer.WebSocket.Net;
using System.Threading.Tasks;

namespace MediaServer.WebSocket.CommandHandlers
{
    sealed class SetTrackInfoCommandHandler : IHandler<IWebSocketRemoteDevice, CommandArgs.SetTrackInfo>
    {
        public async Task HandleAsync(IWebSocketRemoteDevice remoteDevice, SetTrackInfo trackInfo)
        {
            await remoteDevice.SendAsync("TrackInfoSet", null);
        }
    }
}

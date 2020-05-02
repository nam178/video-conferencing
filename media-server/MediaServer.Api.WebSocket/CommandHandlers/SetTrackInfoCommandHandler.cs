using MediaServer.Api.WebSocket.CommandArgs;
using MediaServer.Api.WebSocket.Net;
using MediaServer.Common.Mediator;
using MediaServer.Core.Services.PeerConnection;
using MediaServer.Models;
using System.Threading.Tasks;

namespace MediaServer.Api.WebSocket.CommandHandlers
{
    sealed class SetTrackInfoCommandHandler : IHandler<IWebSocketRemoteDevice, CommandArgs.SetTrackInfo>
    {
        readonly IHandler<IRemoteDevice, SetTrackQualityRequest> _service;

        public SetTrackInfoCommandHandler(IHandler<IRemoteDevice, SetTrackQualityRequest> service)
        {
            _service = service ?? throw new System.ArgumentNullException(nameof(service));
        }

        public async Task HandleAsync(IWebSocketRemoteDevice remoteDevice, SetTrackInfo trackInfo)
        {
            await _service.HandleAsync(remoteDevice, new SetTrackQualityRequest
            {
                TrackId = trackInfo.TrackId,
                TrackQuality = trackInfo.Quality,
                Kind = trackInfo.Kind
            });
            await remoteDevice.SendAsync("TrackInfoSet", null);
        }
    }
}

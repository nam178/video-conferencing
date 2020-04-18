using MediaServer.Common.Mediator;
using MediaServer.Core.Services;
using MediaServer.WebRtc.Managed;
using MediaServer.WebSocket.CommandArgs;
using MediaServer.WebSocket.Net;
using System;
using System.Threading.Tasks;

namespace MediaServer.WebSocket.CommandHandlers
{
    sealed class SetOfferCommandHandler : IHandler<IWebSocketRemoteDevice, CommandArgs.SetOffer>
    {
        readonly IRemoteDeviceService<RTCSessionDescription> _sdpHandler;

        public SetOfferCommandHandler(IRemoteDeviceService<RTCSessionDescription> sdpHandler)
        {
            _sdpHandler = sdpHandler
                ?? throw new ArgumentNullException(nameof(sdpHandler));
        }

        public Task HandleAsync(IWebSocketRemoteDevice remoteDevice, SetOffer args)
        {
            // This command has no response, just pass that to RTC handler
            return _sdpHandler.HandleAsync(remoteDevice, args.Sdp);
        }
    }
}

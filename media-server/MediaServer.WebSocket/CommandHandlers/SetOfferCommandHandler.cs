using MediaServer.Common.Mediator;
using MediaServer.Core.Services;
using MediaServer.Core.Services.PeerConnection;
using MediaServer.WebSocket.CommandArgs;
using MediaServer.WebSocket.Net;
using System;
using System.Threading.Tasks;

namespace MediaServer.WebSocket.CommandHandlers
{
    sealed class SetOfferCommandHandler : IHandler<IWebSocketRemoteDevice, CommandArgs.SetOffer>
    {
        readonly IRemoteDeviceService<PeerConnectionRequest> _rtcHandler;

        public SetOfferCommandHandler(IRemoteDeviceService<PeerConnectionRequest> rtcHander)
        {
            _rtcHandler = rtcHander 
                ?? throw new ArgumentNullException(nameof(rtcHander));
        }

        public Task HandleAsync(IWebSocketRemoteDevice remoteDevice, SetOffer args)
        {
            // This command has no response, just pass that to RTC handler
            return _rtcHandler.HandleAsync(remoteDevice, new PeerConnectionRequest
            {
                OfferedSessionDescription = args.Sdp
            });
        }
    }
}

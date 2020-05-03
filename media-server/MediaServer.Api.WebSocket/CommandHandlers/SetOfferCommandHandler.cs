using MediaServer.Api.WebSocket.CommandArgs;
using MediaServer.Api.WebSocket.Net;
using MediaServer.Common.Mediator;
using MediaServer.Common.Utils;
using MediaServer.Core.Services.PeerConnection;
using System;
using System.Threading.Tasks;

namespace MediaServer.Api.WebSocket.CommandHandlers
{
    sealed class SetOfferCommandHandler : IHandler<IWebSocketRemoteDevice, CommandArgs.SetOffer>
    {
        readonly IRTCSessionDescriptionHandler _sdpHandler;

        public SetOfferCommandHandler(IRTCSessionDescriptionHandler sdpHandler)
        {
            _sdpHandler = sdpHandler
                ?? throw new ArgumentNullException(nameof(sdpHandler));
        }

        public Task HandleAsync(IWebSocketRemoteDevice remoteDevice, SetOffer args)
        {
            Require.NotNull(args.Offer.Sdp);
            Require.NotNull(args.Offer.Type);

            // This command has no response, just pass that to RTC handler
            return _sdpHandler.ReceiveSdpAsync(remoteDevice, args.Offer);
        }
    }
}

using MediaServer.Api.WebSocket.CommandArgs;
using MediaServer.Api.WebSocket.Net;
using MediaServer.Common.Media;
using MediaServer.Common.Patterns;
using MediaServer.Common.Utils;
using MediaServer.Core.Services.Negotiation.Handlers;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MediaServer.Api.WebSocket.CommandHandlers
{
    sealed class SetOfferCommandHandler : IHandler<IWebSocketRemoteDevice, CommandArgs.SetOffer>
    {
        readonly IOfferHandler _offerHandler;

        public SetOfferCommandHandler(IOfferHandler offerHandler)
        {
            _offerHandler = offerHandler
                ?? throw new ArgumentNullException(nameof(offerHandler));
        }

        public async Task HandleAsync(IWebSocketRemoteDevice remoteDevice, SetOffer args)
        {
            Require.NotNull(args.Offer.Sdp);
            Require.NotNull(args.Offer.Type);
            Require.NotNull(args.TransceiverMetadata);

            await _offerHandler.HandleAsync(
                remoteDevice,
                args.PeerConnectionId,
                args.Offer,
                args.TransceiverMetadata.Select(m => (TransceiverMetadata)m).ToArray());
        }
    }
}

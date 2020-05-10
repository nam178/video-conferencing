using MediaServer.Api.WebSocket.CommandArgs;
using MediaServer.Api.WebSocket.Net;
using MediaServer.Common.Mediator;
using MediaServer.Common.Utils;
using MediaServer.Core.Common;
using MediaServer.Core.Services.PeerConnection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MediaServer.Api.WebSocket.CommandHandlers
{
    sealed class SetOfferCommandHandler : IHandler<IWebSocketRemoteDevice, CommandArgs.SetOffer>
    {
        readonly IOfferHandler _offerHandler;
        readonly ITransceiverMetadataSetter _transceiverMetadataHandler;

        public SetOfferCommandHandler(IOfferHandler offerHandler, ITransceiverMetadataSetter transceiverMetadataHandler)
        {
            _offerHandler = offerHandler
                ?? throw new ArgumentNullException(nameof(offerHandler));
            _transceiverMetadataHandler
                = transceiverMetadataHandler ?? throw new ArgumentNullException(nameof(transceiverMetadataHandler));
        }

        public async Task HandleAsync(IWebSocketRemoteDevice remoteDevice, SetOffer args)
        {
            Require.NotNull(args.Offer.Sdp);
            Require.NotNull(args.Offer.Type);
            Require.NotNull(args.TransceiverMetadata);

            await _transceiverMetadataHandler.HandleAsync(remoteDevice,
                args.TransceiverMetadata.Select(m => (TransceiverMetadata)m).ToArray());
            await _offerHandler.HandleAsync(remoteDevice, args.PeerConnectionId, args.Offer);
        }
    }
}

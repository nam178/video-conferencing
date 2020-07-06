using MediaServer.Core.Models;
using MediaServer.Core.Services.Negotiation.MessageQueue;
using System;

namespace MediaServer.Core.Services.RoomManagement
{
    sealed class TransceiverMetadataUpdatedEventObserver : IObserver<TransceiverMetadataUpdatedEvent>
    {
        readonly INegotiationService _negotiationService;

        public TransceiverMetadataUpdatedEventObserver(INegotiationService negotiationService)
        {
            _negotiationService = negotiationService
                ?? throw new ArgumentNullException(nameof(negotiationService));
        }

        public void OnNext(TransceiverMetadataUpdatedEvent args)
        {
            _negotiationService.EnqueueLocalTransceiverMetadata(args.PeerConnection, args.TransceiverMetadata);
        }

        public void OnCompleted() { }

        public void OnError(Exception error) => throw new NotImplementedException();
    }
}

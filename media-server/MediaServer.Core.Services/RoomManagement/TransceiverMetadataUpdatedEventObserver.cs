using MediaServer.Core.Models;
using MediaServer.Core.Services.Negotiation.MessageQueue;
using System;

namespace MediaServer.Core.Services.RoomManagement
{
    sealed class TransceiverMetadataUpdatedEventObserver : IObserver<TransceiverMetadataUpdatedEvent>
    {
        readonly IRoom _room;
        readonly INegotiationService _negotiationService;

        public TransceiverMetadataUpdatedEventObserver(
            IRoom room,
            INegotiationService negotiationService)
        {
            _room = room 
                ?? throw new ArgumentNullException(nameof(room));
            _negotiationService = negotiationService
                ?? throw new ArgumentNullException(nameof(negotiationService));
        }

        public void OnNext(TransceiverMetadataUpdatedEvent args)
        {
            // not assume any thread here.
            // _room.UserProfiles

            // _negotiationService.EnqueueLocalTransceiverMetadata();
        }

        public void OnCompleted() => throw new NotImplementedException();

        public void OnError(Exception error) => throw new NotImplementedException();
    }
}

using MediaServer.Common.Media;
using MediaServer.Core.Models;
using MediaServer.Core.Services.Negotiation.MessageQueue;
using System;
using System.Linq;

namespace MediaServer.Core.Services.Negotiation.Handlers
{
    sealed class AckTransceiverMetadataHandler : IAckTransceiverMetadataHandler
    {
        readonly INegotiationService _negotiationService;

        public AckTransceiverMetadataHandler(INegotiationService negotiationService)
        {
            _negotiationService = negotiationService
                ?? throw new System.ArgumentNullException(nameof(negotiationService));
        }

        public void Handle(IRemoteDevice remoteDevice, TransceiverMetadata metadata)
        {
            // Find the PeerConnection:
            // As the server uses only 1 PeerConnection
            // to send media, 
            // the first PeerConnection is alwaus the one this ack is for.
            var peerConnection = remoteDevice.GetCustomData().PeerConnections.FirstOrDefault();
            if(null == peerConnection)
                throw new InvalidOperationException($"Device {remoteDevice} has no PeerConnection - ack what?");


            _negotiationService.EnqueueLocalTransceiverMetadataAck(peerConnection, metadata);
        }
    }
}

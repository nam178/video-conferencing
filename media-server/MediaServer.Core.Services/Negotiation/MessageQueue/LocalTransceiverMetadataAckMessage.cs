using MediaServer.Common.Media;
using MediaServer.Core.Models;

namespace MediaServer.Core.Services.Negotiation.MessageQueue
{
    sealed class LocalTransceiverMetadataAckMessage : Message
    {
        public TransceiverMetadata Acked { get; }

        public LocalTransceiverMetadataAckMessage(IPeerConnection peerConnection, TransceiverMetadata transceiverMetadata)
            : base(peerConnection)
        {
            Acked = transceiverMetadata;
        }
    }
}
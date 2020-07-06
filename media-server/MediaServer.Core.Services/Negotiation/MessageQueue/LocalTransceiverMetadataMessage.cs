using MediaServer.Common.Media;
using MediaServer.Core.Models;

namespace MediaServer.Core.Services.Negotiation.MessageQueue
{
    sealed class LocalTransceiverMetadataMessage : Message
    {
        public TransceiverMetadata TransceiverMetadata { get; }

        public LocalTransceiverMetadataMessage(TransceiverMetadata transceiverMetadata, IPeerConnection peerConnection)
            : base(peerConnection)
        {
            TransceiverMetadata = transceiverMetadata;
        }
    }
}
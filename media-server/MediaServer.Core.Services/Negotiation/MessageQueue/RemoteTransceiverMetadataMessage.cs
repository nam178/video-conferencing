using MediaServer.Common.Media;
using MediaServer.Core.Models;
using System.Collections.Generic;

namespace MediaServer.Core.Services.Negotiation.MessageQueue
{
    public sealed class RemoteTransceiverMetadataMessage : Message
    {
        public IReadOnlyList<TransceiverMetadata> Transceivers { get; }

        public RemoteTransceiverMetadataMessage(IReadOnlyList<TransceiverMetadata> transceiverMetadata, IPeerConnection peerConnection)
            : base(peerConnection)
        {
            Transceivers = transceiverMetadata
                ?? throw new System.ArgumentNullException(nameof(transceiverMetadata));
        }
    }
}

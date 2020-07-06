using MediaServer.Api.WebSocket.Models;
using MediaServer.WebRtc.Common;
using System;

namespace MediaServer.Api.WebSocket.CommandArgs
{
    public sealed class SetOffer
    {
        public RTCSessionDescription Offer { get; set; }

        public Guid? PeerConnectionId { get; set; }

        public WsTransceiverMetadata[] TransceiverMetadata { get; set; }
    }
}

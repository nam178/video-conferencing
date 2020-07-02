using MediaServer.Common.Media;
using MediaServer.WebRtc.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace MediaServer.Api.WebSocket.CommandArgs
{
    public sealed class SetOffer
    {
        public RTCSessionDescription Offer { get; set; }

        public Guid? PeerConnectionId { get; set; }

        public WebSocketTransceiverMetadata[] TransceiverMetadata { get; set; }

        public sealed class WebSocketTransceiverMetadata
        {
            public string TransceiverMid { get; set; }

            [JsonConverter(typeof(StringEnumConverter))]
            public MediaQuality Quality { get; set; }

            [JsonConverter(typeof(StringEnumConverter))]
            public MediaKind Kind { get; set; }
        }
    }
}

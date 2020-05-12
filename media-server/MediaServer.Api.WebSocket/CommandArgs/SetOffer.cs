using MediaServer.Core.Common;
using MediaServer.WebRtc.Managed;
using MediaServer.WebRtc.MediaRouting;
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

            public static implicit operator TransceiverMetadata(WebSocketTransceiverMetadata transceiverInfo)
            {
                return new TransceiverMetadata(
                    transceiverInfo.TransceiverMid,
                    transceiverInfo.Quality,
                    transceiverInfo.Kind
                    );
            }
        }
    }
}

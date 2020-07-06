using MediaServer.Common.Media;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace MediaServer.Api.WebSocket.Models
{
    public sealed class WsTransceiverMetadata
    {
        [JsonProperty("transceiverMid")]
        public string TransceiverMid { get; set; }

        [JsonProperty("quality")]
        [JsonConverter(typeof(StringEnumConverter))]
        public MediaQuality Quality { get; set; }

        [JsonProperty("kind")]
        [JsonConverter(typeof(StringEnumConverter))]
        public MediaKind Kind { get; set; }

        [JsonProperty("sourceDeviceId")]
        public Guid? SourceDeviceId { get; set; }

        public WsTransceiverMetadata()
        {

        }

        public WsTransceiverMetadata(TransceiverMetadata other)
        {
            TransceiverMid = other.TransceiverMid;
            Quality = other.Quality;
            Kind = other.Kind;
            SourceDeviceId = other.SourceDeviceId == Guid.Empty ? (Guid?)null : other.SourceDeviceId;
        }

        public static implicit operator TransceiverMetadata(WsTransceiverMetadata me)
            => new TransceiverMetadata(me.TransceiverMid, me.Quality, me.Kind, me.SourceDeviceId ?? Guid.Empty);
    }
}

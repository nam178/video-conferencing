using MediaServer.Common.Utils;
using Newtonsoft.Json;
using System;

namespace MediaServer.Common.Media
{
    public sealed class TransceiverMetadata
    {
        /// <summary>
        /// The unique identifier for this transceiver within the context of an SDP
        /// </summary>
        [JsonProperty("transceiverMid")]
        public string TransceiverMid { get; }

        /// <summary>
        /// The quality level of content
        /// </summary>
        [JsonProperty("trackQuality")]
        public MediaQuality TrackQuality { get; }

        /// <summary>
        /// Kind of content
        /// </summary>
        [JsonProperty("kind")]
        public MediaKind Kind { get; }

        /// <summary>
        /// The device id in which the media for this transceiver is produced from.
        /// </summary>
        [JsonProperty("sourceDeviceId")]
        public Guid SourceDeviceId { get; }

        public TransceiverMetadata(string mid, MediaQuality quality, MediaKind kind, Guid sourceDeviceId)
        {
            Require.NotNullOrWhiteSpace(mid);
            Require.NotEmpty(sourceDeviceId);

            TransceiverMid = mid.Trim();
            TrackQuality = quality;
            Kind = kind;
            SourceDeviceId = sourceDeviceId;
        }
    }
}

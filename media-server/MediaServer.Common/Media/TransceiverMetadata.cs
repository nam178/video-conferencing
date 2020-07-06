using MediaServer.Common.Utils;
using System;

namespace MediaServer.Common.Media
{
    public sealed class TransceiverMetadata
    {
        /// <summary>
        /// The unique identifier for this transceiver within the context of an SDP
        /// </summary>
        public string TransceiverMid { get; }

        /// <summary>
        /// The quality level of content
        /// </summary>
        public MediaQuality Quality { get; }

        /// <summary>
        /// Kind of content
        /// </summary>
        public MediaKind Kind { get; }

        /// <summary>
        /// The device id in which the media for this transceiver is produced from.
        /// </summary>
        public Guid SourceDeviceId { get; }

        public TransceiverMetadata(string mid, MediaQuality quality, MediaKind kind, Guid sourceDeviceId)
        {
            Require.NotNullOrWhiteSpace(mid);
            Require.NotEmpty(sourceDeviceId);

            TransceiverMid = mid.Trim();
            Quality = quality;
            Kind = kind;
            SourceDeviceId = sourceDeviceId;
        }
    }
}

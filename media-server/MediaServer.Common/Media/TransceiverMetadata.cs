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
        public MediaQuality TrackQuality { get; }

        /// <summary>
        /// Kind of content
        /// </summary>
        public MediaKind Kind { get; }

        /// <summary>
        /// The device id in which the content comes from or NULL when unspecified
        /// </summary>
        public Guid? SourceDeviceId { get; }

        public TransceiverMetadata(string mid, MediaQuality quality, MediaKind kind)
        {
            if(string.IsNullOrWhiteSpace(mid))
                throw new System.ArgumentException("Mid cannot be NULL or empty", nameof(mid));
            TransceiverMid = mid;
            TrackQuality = quality;
            Kind = kind;
        }

        public TransceiverMetadata(string mid, MediaQuality quality, MediaKind kind, Guid sourceDeviceId)
            : this(mid, quality, kind)
        {
            SourceDeviceId = sourceDeviceId;
        }
    }
}

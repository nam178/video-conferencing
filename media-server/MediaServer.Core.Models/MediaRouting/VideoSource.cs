using MediaServer.Common.Media;
using MediaServer.WebRtc.Managed;
using System;

namespace MediaServer.Core.Models.MediaRouting
{
    sealed class VideoSource
    {
        /// <summary>
        /// The remote transceiver that this source expects to receive.
        /// </summary>
        public string ExpectedTransceiverMid { get; set; }

        /// <summary>
        /// The VideoClient that owns this source
        /// </summary>
        public VideoClient VideoClient { get; }

        /// <summary>
        /// Quality of this source
        /// </summary>
        public MediaQuality Quality { get; }

        public VideoSource(VideoClient videoClient, MediaQuality quality)
        {
            VideoClient = videoClient ?? throw new ArgumentNullException(nameof(videoClient));
            Quality = quality;
        }

        public PassiveVideoTrackSource VideoTrackSource { get; set; }

        public VideoSinkAdapter VideoSinkAdapter { get; set; }

        public override string ToString() => $"[VideoSource {VideoClient.Id.ToString().Substring(0, 8)}-{Quality}]";
    }
}

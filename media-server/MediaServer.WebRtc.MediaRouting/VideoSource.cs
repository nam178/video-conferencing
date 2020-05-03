using MediaServer.WebRtc.Managed;
using System;

namespace MediaServer.WebRtc.MediaRouting
{
    sealed class VideoSource
    {
        /// <summary>
        /// The remote track that this source expects to receive.
        /// </summary>
        public string ExpectedTrackId { get; set; }

        /// <summary>
        /// The VideoClient that owns this source
        /// </summary>
        public VideoClient VideoClient { get; }

        public TrackQuality TrackQuality { get; }

        public VideoSource(VideoClient videoClient, TrackQuality trackQuality)
        {
            VideoClient = videoClient ?? throw new ArgumentNullException(nameof(videoClient));
            TrackQuality = trackQuality;
        }

        public PassiveVideoTrackSource VideoTrackSource { get; set; }

        public VideoSinkAdapter VideoSinkAdapter { get; set; }

        public override string ToString() => $"[VideoSource {VideoClient.Id.ToString().Substring(0, 8)}-{TrackQuality}]";
    }
}

using MediaServer.WebRtc.Managed;

namespace MediaServer.Core.Services.MediaRouting
{
    sealed class VideoSource
    {
        public string ExpectedTrackId { get; set; }

        public PassiveVideoTrackSource VideoTrackSource { get; set; }

        public VideoSinkAdapter VideoSinkAdapter { get; set; }
    }
}

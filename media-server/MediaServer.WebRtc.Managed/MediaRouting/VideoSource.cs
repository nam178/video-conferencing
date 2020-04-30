using System.Collections.Generic;

namespace MediaServer.WebRtc.Managed.MediaRouting
{
    sealed class VideoSource
    {
        public string ExpectedTrackId { get; set; }

        public RtpReceiver ConnectedRemoteTrack { get; set; }

        public PassiveVideoTrackSource VideoTrackSource { get; set; }

        public VideoSinkAdapter VideoSinkAdapter { get; set; }
    }
}

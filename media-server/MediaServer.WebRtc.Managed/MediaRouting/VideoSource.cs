namespace MediaServer.WebRtc.Managed.MediaRouting
{
    sealed class VideoSource
    {
        public string ExpectedTrackId { get; set; }

        public RtpReceiver ConnectedTrack { get; set; }

        public PassiveVideoTrackSource VideoTrackSource { get; set; }

        public VideoSinkAdapter VideoSinkAdapter { get; set; }
    }
}

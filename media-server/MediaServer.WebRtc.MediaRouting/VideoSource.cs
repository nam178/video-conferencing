using MediaServer.WebRtc.Managed;

namespace MediaServer.WebRtc.MediaRouting
{
    sealed class VideoSource
    {
        readonly string _debugName;

        public string ExpectedTrackId { get; set; }

        public VideoSource(string debugName)
        {
            _debugName = debugName ?? throw new System.ArgumentNullException(nameof(debugName));
        }

        public PassiveVideoTrackSource VideoTrackSource { get; set; }

        public VideoSinkAdapter VideoSinkAdapter { get; set; }

        public override string ToString() => $"[VideoSource {_debugName}]";
    }
}

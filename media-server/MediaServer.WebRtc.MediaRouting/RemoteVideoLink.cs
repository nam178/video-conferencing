using MediaServer.WebRtc.Managed;

namespace MediaServer.WebRtc.MediaRouting
{
    sealed class RemoteVideoLink
    {
        public VideoSource VideoSource { get; }

        public RtpReceiver RemoteTrack { get; }

        public RemoteVideoLink(VideoSource videoSource, RtpReceiver remoteTrack)
        {
            VideoSource = videoSource
                ?? throw new System.ArgumentNullException(nameof(videoSource));
            RemoteTrack = remoteTrack
                ?? throw new System.ArgumentNullException(nameof(remoteTrack));
        }

        public override string ToString() => $"[RemoteVideoLink Src={RemoteTrack}, Dst={VideoSource}]";
    }
}

using MediaServer.WebRtc.Managed;

namespace MediaServer.Core.Models.MediaRouting
{
    /// <summary>
    /// Represents a link between remote video track and our local VideoSource
    /// </summary>
    sealed class RemoteVideoLink
    {
        /// <summary>
        /// The local VideoSource that receive video frames
        /// </summary>
        public VideoSource VideoSource { get; }

        /// <summary>
        /// The remote track that provides video frames for our local source
        /// </summary>
        public RtpReceiver RemoteTrack { get; }

        /// <summary>
        /// The PeerConnection in which the RemoteTrack belongs to
        /// </summary>
        public IPeerConnection PeerConnection { get; }

        public RemoteVideoLink(IPeerConnection peerConnection, VideoSource videoSource, RtpReceiver remoteTrack)
        {
            PeerConnection = peerConnection
                ?? throw new System.ArgumentNullException(nameof(peerConnection));
            VideoSource = videoSource
                ?? throw new System.ArgumentNullException(nameof(videoSource));
            RemoteTrack = remoteTrack
                ?? throw new System.ArgumentNullException(nameof(remoteTrack));
        }

        public override string ToString() => $"[RemoteVideoLink Src={RemoteTrack}, Dst={VideoSource}]";
    }
}

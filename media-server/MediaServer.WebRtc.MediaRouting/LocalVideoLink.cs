using MediaServer.WebRtc.Managed;
using NLog;
using System;
using System.Threading;

namespace MediaServer.WebRtc.MediaRouting
{
    sealed class LocalVideoLink : IDisposable
    {
        readonly VideoTrack _track;
        readonly RtpSender _rtpSender;
        readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        public PeerConnection TargetPeerConnection { get; }

        public VideoSource VideoSource { get; }

        public LocalVideoLink(PeerConnectionFactory peerConnectionFactory, VideoSource videoSource, PeerConnection targetPeerConnection)
        {
            if(peerConnectionFactory is null)
                throw new ArgumentNullException(nameof(peerConnectionFactory));
            if(videoSource is null)
                throw new ArgumentNullException(nameof(videoSource));
            if(targetPeerConnection is null)
                throw new ArgumentNullException(nameof(targetPeerConnection));
            if(null == videoSource.VideoTrackSource)
                throw new InvalidProgramException("VideoTrackSource is NULL");

            TargetPeerConnection = targetPeerConnection;
            VideoSource = videoSource;

            // Create tracks, for simplicity, the stream id MUST be the same as the video client ID.
            // This is very important, so that the remote clients know which track belongs to which client.
            var trackId = Guid.NewGuid();
            var streamId = videoSource.VideoClient.Id;
            _track = peerConnectionFactory.CreateVideoTrack(trackId.ToString(), videoSource.VideoTrackSource);

            // Add track to peer
            _rtpSender = TargetPeerConnection.AddTrack(_track, streamId);
            _logger.Debug($"Local track created {_track}");
        }

        int _close;

        public void Close()
        {
            if(Interlocked.CompareExchange(ref _close, 1, 0) != 0)
                throw new InvalidOperationException("Already Closed");

            TargetPeerConnection.RemoveTrack(_rtpSender);
        }

        public void Dispose()
        {
            // Notes - Dispose only the track,
            // Don't dispose RtpSender, the PeerConnection owns it.
            _track.Dispose();
        }

        public override string ToString() => $"[LocalVideoLink Source={VideoSource}, Target={TargetPeerConnection}, Track={_track}]";
    }
}

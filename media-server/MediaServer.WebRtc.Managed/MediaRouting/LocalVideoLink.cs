using System;
using System.Threading;

namespace MediaServer.WebRtc.Managed.MediaRouting
{
    sealed class LocalVideoLink : IDisposable
    {
        readonly VideoTrack _track;
        readonly RtpSender _rtpSender;

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

            // Create track and own it
            var trackId = Guid.NewGuid();
            var streamId = Guid.NewGuid();
            _track = peerConnectionFactory.CreateVideoTrack(trackId.ToString(), videoSource.VideoTrackSource);

            // Add track to peer
            _rtpSender = TargetPeerConnection.AddTrack(_track, streamId);
            
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
    }
}

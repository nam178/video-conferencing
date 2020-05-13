using MediaServer.WebRtc.Managed;
using NLog;
using System;
using System.Threading;

namespace MediaServer.WebRtc.MediaRouting
{
    sealed class LocalVideoLink : IDisposable
    {
        readonly VideoTrack _track;
        readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        readonly RtpTransceiver _transceiver;

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

            // Find the first available transceiver,
            // Or add a new one
            var mediaKind = _track.Kind;
            var transceivers = TargetPeerConnection.GetTransceivers();
            _transceiver = null;
            for(var i = 0; i < transceivers.Count; i++)
            {
                if(transceivers[i].ReusabilityState == ReusabilityState.Available
                    && transceivers[i].MediaKind == mediaKind)
                {
                    _transceiver = transceivers[i];
                    break;
                }
            }
            _transceiver = _transceiver ?? TargetPeerConnection.AddTransceiver(mediaKind);

            // Notes: AddTransceiver() triggers re-negotation, so does ToBusyState();
            // Need to test to confirm we won't re-negotiate multiple times

            // Next, set/replace the track:
            _transceiver.ToBusyState(_track, streamId);

            // Add track to peer
            _logger.Debug($"Local track created {_track}");
        }

        int _close;

        public void Close()
        {
            if(Interlocked.CompareExchange(ref _close, 1, 0) != 0)
                throw new InvalidOperationException("Already Closed");
            _transceiver.ToFrozenState();
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

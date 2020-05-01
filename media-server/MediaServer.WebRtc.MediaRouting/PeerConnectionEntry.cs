using MediaServer.WebRtc.Managed;
using System;

namespace MediaServer.WebRtc.MediaRouting
{
    sealed class PeerConnectionEntry
    {
        public PeerConnection PeerConnection { get; }
        public PeerConnectionObserver PeerConnectionObserver { get; }

        public PeerConnectionEntry(
            PeerConnection peerConnection,
            PeerConnectionObserver peerConnectionObserver)
        {
            PeerConnection = peerConnection
                ?? throw new ArgumentNullException(nameof(peerConnection));
            PeerConnectionObserver = peerConnectionObserver
                ?? throw new ArgumentNullException(nameof(peerConnectionObserver));
        }
    }
}

using MediaServer.WebRtc.Managed;
using System;

namespace MediaServer.Core.Services.MediaRouting
{
    sealed class PeerConnectionEntry
    {
        public global::MediaServer.WebRtc.Managed.PeerConnection PeerConnection { get; }
        public PeerConnectionObserver PeerConnectionObserver { get; }

        public PeerConnectionEntry(
            global::MediaServer.WebRtc.Managed.PeerConnection peerConnection,
            PeerConnectionObserver peerConnectionObserver)
        {
            PeerConnection = peerConnection
                ?? throw new ArgumentNullException(nameof(peerConnection));
            PeerConnectionObserver = peerConnectionObserver
                ?? throw new ArgumentNullException(nameof(peerConnectionObserver));
        }
    }
}

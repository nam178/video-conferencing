using System.Runtime.InteropServices;

namespace MediaServer.WebRtc.Managed
{
    static partial class PeerConnectionObserverInterop
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct PeerConnectionObserverCallbacks
        {
            public RenegotiationNeededCallback RenegotiationNeededCallback;
            public IceGatheringStateChangedCallback IceGatheringStateChangedCallback;
            public IceConnectionChangeCallback IceConnectionChangeCallback;
            public IceCandidateCallback IceCandidateCallback;
            public IceCandidatesRemovedCallback IceCandidatesRemovedCallback;
            public RemoteTrackAddedCallback RemoteTrackAddedCallback;
            public RemoteTrackRemovedCallback RemoteTrackRemovedCallback;
        };
    }
}

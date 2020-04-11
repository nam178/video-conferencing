using MediaServer.Common.Utils;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using static MediaServer.WebRtc.Managed.PeerConnectionObserverInterop;

namespace MediaServer.WebRtc.Managed
{
    public sealed class PeerConnectionObserver : IDisposable
    {
        internal PeerConnectionObserverSafeHandle Native { get; }

        readonly GCHandle _handle;
        readonly RenegotiationNeededCallback _renegotiationNeededCallback = RenegotiationNeededCallback;
        readonly IceGatheringStateChangedCallback _iceGatheringStateChangedCallback = IceGatheringStateChangedCallback;
        readonly IceConnectionChangeCallback _iceConnectionChangeCallback = IceConnectionChangeCallback;
        readonly IceCandidateCallback _iceCandidateCallback = IceCandidateCallback;
        readonly IceCandidatesRemovedCallback _iceCandidatesRemovedCallback = IceCandidatesRemovedCallback;
        readonly RemoteTrackAddedCallback _remoteTrackAddedCallback = RemoteTrackAddedCallback;
        readonly RemoteTrackRemovedCallback _remoteTrackRemovedCallback = RemoteTrackRemovedCallback;

        public event EventHandler<EventArgs<IceCandidate>> IceCandidateAdded;
        public event EventHandler<EventArgs<IceConnectionState>> IceConnectionStateChanged;
        public event EventHandler<EventArgs<IceGatheringState>> IceGatheringStateChanged;
        public event EventHandler<EventArgs> RenegotiationNeeded;
        public event EventHandler<EventArgs<IntPtr>> RemoteTrackRemoved;
        public event EventHandler<EventArgs<IntPtr>> RemoteTrackAdded;
        public event EventHandler<EventArgs<IntPtr>> IceCandidatesRemoved;

        public PeerConnectionObserver()
        {
            Native = new PeerConnectionObserverSafeHandle();

            var userData = GCHandleHelper.ToIntPtr(this, out _handle);

            SetCallbacks(Native, userData, new PeerConnectionObserverCallbacks
            {
                RenegotiationNeededCallback = _renegotiationNeededCallback,
                IceGatheringStateChangedCallback = _iceGatheringStateChangedCallback,
                IceConnectionChangeCallback = _iceConnectionChangeCallback,
                IceCandidateCallback = _iceCandidateCallback,
                IceCandidatesRemovedCallback = _iceCandidatesRemovedCallback,
                RemoteTrackAddedCallback = _remoteTrackAddedCallback,
                RemoteTrackRemovedCallback = _remoteTrackRemovedCallback
            });
        }

        static void IceCandidateCallback(IntPtr userData, IceCandidate iceCandidate)
        {
            var source = GCHandleHelper.FromIntPtr<PeerConnectionObserver>(userData);
            source?.IceCandidateAdded(source, new EventArgs<IceCandidate>(iceCandidate));
        }

        static void IceConnectionChangeCallback(IntPtr userData, IceConnectionState state)
        {
            var source = GCHandleHelper.FromIntPtr<PeerConnectionObserver>(userData);
            source?.IceConnectionStateChanged(source, new EventArgs<IceConnectionState>(state));
        }

        static void IceGatheringStateChangedCallback(IntPtr userData, IceGatheringState state)
        {
            var source = GCHandleHelper.FromIntPtr<PeerConnectionObserver>(userData);
            source?.IceGatheringStateChanged(source, new EventArgs<IceGatheringState>(state));
        }

        static void RenegotiationNeededCallback(IntPtr userData)
        {
            var source = GCHandleHelper.FromIntPtr<PeerConnectionObserver>(userData);
            source?.RenegotiationNeeded(source, EventArgs.Empty);
        }

        static void RemoteTrackRemovedCallback(IntPtr userData, IntPtr rtpReceiverInterfacePtr)
        {
            var source = GCHandleHelper.FromIntPtr<PeerConnectionObserver>(userData);
            source?.RemoteTrackRemoved(source, new EventArgs<IntPtr>(rtpReceiverInterfacePtr));
        }

        static void RemoteTrackAddedCallback(IntPtr userData, IntPtr rtpTransceiverInterfacePtr)
        {
            var source = GCHandleHelper.FromIntPtr<PeerConnectionObserver>(userData);
            source?.RemoteTrackAdded(source, new EventArgs<IntPtr>(rtpTransceiverInterfacePtr));
        }

        static void IceCandidatesRemovedCallback(IntPtr userData, IntPtr candidates)
        {
            var source = GCHandleHelper.FromIntPtr<PeerConnectionObserver>(userData);
            source?.IceCandidatesRemoved(source, new EventArgs<IntPtr>(candidates));
        }

        int _disposed = 0;
        public void Dispose()
        {
            if(Interlocked.CompareExchange(ref _disposed, 1, 0) == 0)
            {
                Native.Dispose();
                _handle.Free();
            }
        }
    }
}

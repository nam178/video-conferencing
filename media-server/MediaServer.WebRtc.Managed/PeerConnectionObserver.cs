using MediaServer.Common.Utils;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
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
        readonly List<RtpReceiver> _remoteTracks = new List<RtpReceiver>();
        readonly ILogger _logger = NLog.LogManager.GetCurrentClassLogger();

        public event EventHandler<EventArgs<RTCIceCandidate>> IceCandidateAdded;
        public event EventHandler<EventArgs<RTCIceConnectionState>> IceConnectionStateChanged;
        public event EventHandler<EventArgs<RTCIceGatheringState>> IceGatheringStateChanged;
        public event EventHandler<EventArgs> RenegotiationNeeded;
        public event EventHandler<EventArgs<RtpReceiver>> RemoteTrackRemoved;
        public event EventHandler<EventArgs<RtpReceiver>> RemoteTrackAdded;
        public event EventHandler<EventArgs<IntPtr>> IceCandidatesRemoved;

        public IReadOnlyList<RtpReceiver> RemoteTracks
        {
            get
            {
                CheckDisposed();
                return _remoteTracks;
            }
        }

        public PeerConnectionObserver()
        {
            Native = new PeerConnectionObserverSafeHandle();

            var userData = GCHandleHelper.ToIntPtr(this, out _handle);

            SetRenegotiationNeededCallback(Native, _renegotiationNeededCallback, userData);
            SetIceGatheringStateChangedCallback(Native, _iceGatheringStateChangedCallback, userData);
            SetIceConnectionChangeCallback(Native, _iceConnectionChangeCallback, userData);
            SetIceCandidateCallback(Native, _iceCandidateCallback, userData);
            SetIceCandidatesRemovedCallback(Native, _iceCandidatesRemovedCallback, userData);
            SetRemoteTrackAddedCallback(Native, _remoteTrackAddedCallback, userData);
            SetRemoteTrackRemovedCallback(Native, _remoteTrackRemovedCallback, userData);
        }

        void OnHandleRemoveTrackAdded(IntPtr rtpReceiverWrapperPtr) // signalling thread
        {
            CheckDisposed();

            // Take ownership of the new RtpReceiver 
            var remoteTrack = new RtpReceiver(rtpReceiverWrapperPtr);
            _remoteTracks.Add(remoteTrack);

            // Trigger event
            RemoteTrackAdded?.Invoke(this, new EventArgs<RtpReceiver>(remoteTrack));
            _logger.Debug($"Remote track added: {remoteTrack}");
        }

        void OnHandleRemoveTrackRemoved(IntPtr rtpReceiverPtr) // signalling thread
        {
            CheckDisposed();

            // Find the RTP Receiver wrappers that manages this native webRTC rtpReceiverPtr
            // and destroy them.
            foreach(var remoteTrack in _remoteTracks.Where(r => r.GetRtpReceiverInterface() == rtpReceiverPtr).ToList())
            {
                using(remoteTrack)
                {
                    RemoteTrackRemoved?.Invoke(this, new EventArgs<RtpReceiver>(remoteTrack));
                    _remoteTracks.Remove(remoteTrack);
                    _logger.Debug($"Remote track removed: {remoteTrack}");
                }
            }
        }

        static void IceCandidateCallback(IntPtr userData, IceCandidate iceCandidate)
        {
            var source = GCHandleHelper.FromIntPtr<PeerConnectionObserver>(userData);
            source?.IceCandidateAdded?.Invoke(
                source,
                new EventArgs<RTCIceCandidate>(new RTCIceCandidate(iceCandidate)));
        }

        static void IceConnectionChangeCallback(IntPtr userData, RTCIceConnectionState state)
        {
            var source = GCHandleHelper.FromIntPtr<PeerConnectionObserver>(userData);
            source?.IceConnectionStateChanged?.Invoke(source, new EventArgs<RTCIceConnectionState>(state));
        }

        static void IceGatheringStateChangedCallback(IntPtr userData, RTCIceGatheringState state)
        {
            var source = GCHandleHelper.FromIntPtr<PeerConnectionObserver>(userData);
            source?.IceGatheringStateChanged?.Invoke(source, new EventArgs<RTCIceGatheringState>(state));
        }

        static void RenegotiationNeededCallback(IntPtr userData)
        {
            var source = GCHandleHelper.FromIntPtr<PeerConnectionObserver>(userData);
            source?.RenegotiationNeeded?.Invoke(source, EventArgs.Empty);
        }

        static void RemoteTrackRemovedCallback(IntPtr userData, IntPtr rtpReceiverPtr)
        {
            var source = GCHandleHelper.FromIntPtr<PeerConnectionObserver>(userData);
            source.OnHandleRemoveTrackRemoved(rtpReceiverPtr);
        }

        static void RemoteTrackAddedCallback(IntPtr userData, IntPtr rtpReceiverWrapperPtr)
        {
            var source = GCHandleHelper.FromIntPtr<PeerConnectionObserver>(userData);
            source?.OnHandleRemoveTrackAdded(rtpReceiverWrapperPtr);
        }

        static void IceCandidatesRemovedCallback(IntPtr userData, IntPtr candidates)
        {
            var source = GCHandleHelper.FromIntPtr<PeerConnectionObserver>(userData);
            source?.IceCandidatesRemoved?.Invoke(source, new EventArgs<IntPtr>(candidates));
        }

        void CheckDisposed()
        {
            if(Interlocked.CompareExchange(ref _disposed, 0, 0) == 1)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

        int _disposed = 0;
        public void Dispose()
        {
            if(Interlocked.CompareExchange(ref _disposed, 1, 0) == 0)
            {
                foreach(var t in _remoteTracks)
                {
                    t.Dispose();
                }

                Native.Dispose();
                _handle.Free();
            }
        }
    }
}

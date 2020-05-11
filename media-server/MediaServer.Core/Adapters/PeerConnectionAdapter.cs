using MediaServer.Common.Patterns;
using MediaServer.Common.Utils;
using MediaServer.Core.Models;
using MediaServer.Models;
using MediaServer.WebRtc.Managed;
using MediaServer.WebRtc.MediaRouting;
using NLog;
using System;
using System.Collections.Generic;
using System.Threading;

namespace MediaServer.Core.Adapters
{
    sealed class PeerConnectionAdapter : IPeerConnection
    {
        readonly PeerConnectionObserver _peerConnectionObserverImpl;
        readonly PeerConnection _peerConnectionImpl;
        readonly object _syncRoot = new object();
        readonly VideoRouter _videoRouter;
        readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        Action<IPeerConnection, RTCIceCandidate> _iceCandidateObserver;
        Action<IPeerConnection> _renegotationNeededObserver;
        int _addedToRouterState;

        enum AddedToRouterState : int
        {
            NotAdded = 0,
            Added = 1,
            Removed = 2
        }

        public IRoom Room { get; }

        public IRemoteDevice Device { get; }

        public Guid Id => _peerConnectionImpl.Id;

        public Guid LastOfferId { get; private set; }

        public PeerConnectionAdapter(
            PeerConnectionFactory peerConnectionFactory,
            VideoRouter videoRouter,
            IRoom room,
            IRemoteDevice device,
            IReadOnlyList<string> stunUrls)
        {
            if(peerConnectionFactory is null)
                throw new ArgumentNullException(nameof(peerConnectionFactory));
            if(stunUrls is null)
                throw new ArgumentNullException(nameof(stunUrls));
            Room = room ?? throw new ArgumentNullException(nameof(room));
            Device = device ?? throw new ArgumentNullException(nameof(device));
            _videoRouter = videoRouter
                ?? throw new ArgumentNullException(nameof(videoRouter));
            var iceServerInfo = new PeerConnectionConfig.IceServerInfo();
            foreach(var url in stunUrls)
            {
                iceServerInfo.Urls.Add(url);
            }
            var config = new PeerConnectionConfig();
            config.IceServers.Add(iceServerInfo);
            _peerConnectionObserverImpl = new PeerConnectionObserver();
            _peerConnectionObserverImpl.IceCandidateAdded += IceCandidateAdded;
            _peerConnectionObserverImpl.RenegotiationNeeded += RenegotationNeeded;
            _peerConnectionImpl = peerConnectionFactory.CreatePeerConnection(_peerConnectionObserverImpl, config);
        }


        public void SetRemoteSessionDescription(RTCSessionDescription description, Observer observer)
        {
            if(observer is null)
                throw new ArgumentNullException(nameof(observer));
            MustNotDisposed();
            _peerConnectionImpl.SetRemoteSessionDescription(
                description.Type,
                description.Sdp,
                new Observer().OnSuccess(delegate
                {
                    try
                    {
                        // As per webRTC example, the answerer will SetRemoteSessionDescription() first,
                        // then followed by AddTrack();
                        //
                        // and AddPeerConnectionAsync() will call AddTrack() under the hood,
                        // therefore we call AddPeerConnectionAsync() right after SetRemoteSessionDescription();
                        if(Interlocked.CompareExchange(
                            ref _addedToRouterState,
                            (int)AddedToRouterState.Added,
                            (int)AddedToRouterState.NotAdded) == (int)AddedToRouterState.NotAdded)
                        {
                            _videoRouter.AddPeerConnection(Device.Id, _peerConnectionImpl);
                        }
                        observer.Success();
                    }
                    catch(Exception ex)
                    {
                        observer.Error(ex.Message);
                    }

                }).OnError(msg => observer.Error(msg)));
        }

        public void CreateOffer(Observer<RTCSessionDescription> observer)
        {
            if(observer is null)
                throw new ArgumentNullException(nameof(observer));
            MustNotDisposed();
            LastOfferId = Guid.NewGuid();
            _peerConnectionImpl.CreateOffer(observer);
        }

        public void CreateAnswer(Observer<RTCSessionDescription> observer)
        {
            MustNotDisposed();
            _peerConnectionImpl.CreateAnswer(observer);
        }

        public void SetLocalSessionDescription(RTCSessionDescription localDescription, Observer observer)
        {
            MustNotDisposed();
            // After generating answer, must set LocalSdp,
            // otherwise ICE candidates won't be gathered.
            _peerConnectionImpl.SetLocalSessionDescription(
                localDescription.Type,
                localDescription.Sdp,
                observer);
        }

        public void AddIceCandidate(RTCIceCandidate iceCandidate)
        {
            MustNotDisposed();
            _peerConnectionImpl.AddIceCandidate(iceCandidate);
        }

        public IPeerConnection ObserveIceCandidate(Action<IPeerConnection, RTCIceCandidate> observer)
        {
            MustNotDisposed();
            lock(_syncRoot)
            {
                if(_iceCandidateObserver != null)
                {
                    throw new NotSupportedException("Only one observer supported sorry");
                }
                _iceCandidateObserver = observer;
            }
            return this;
        }

        public IPeerConnection ObserveRenegotiationNeeded(Action<IPeerConnection> observer)
        {
            MustNotDisposed();
            lock(_syncRoot)
            {
                if(_renegotationNeededObserver != null)
                    throw new NotSupportedException("Only one observer supported sorry");
                _renegotationNeededObserver = observer;
            }
            return this;
        }

        public void Close()
        {
            MustNotDisposed();
            if(Interlocked.CompareExchange(
                ref _addedToRouterState,
                (int)AddedToRouterState.Removed,
                (int)AddedToRouterState.Added) == (int)AddedToRouterState.Added)
            {
                _videoRouter.RemovePeerConnection(Device.Id, _peerConnectionImpl, _peerConnectionObserverImpl);
            }
            _peerConnectionImpl.Close();
        }

        int _disposed;
        public void Dispose()
        {
            if(Interlocked.CompareExchange(ref _disposed, 1, 0) == 0)
            {
                // Order matters! note that PeerConnection must be closed first by calling Close() above.
                _peerConnectionImpl.Dispose();
                // Observer later, cuz PeerConnection uses it
                _peerConnectionObserverImpl.IceCandidateAdded -= IceCandidateAdded;
                _peerConnectionObserverImpl.RenegotiationNeeded -= RenegotationNeeded;
                _peerConnectionObserverImpl.Dispose();
            }
        }

        public override string ToString() => $"[PeerConnectionAdapter Id={Id.ToString().Substring(0, 8)}]";

        void IceCandidateAdded(object sender, EventArgs<RTCIceCandidate> e) => _iceCandidateObserver?.Invoke(this, e.Payload);

        void RenegotationNeeded(object sender, EventArgs e) => _renegotationNeededObserver?.Invoke(this);

        void MustNotDisposed()
        {
            if(Interlocked.CompareExchange(ref _disposed, 0, 0) == 1)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }
    }
}

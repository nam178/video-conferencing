﻿using MediaServer.Common.Patterns;
using MediaServer.Common.Utils;
using MediaServer.Core.Models.MediaRouting;
using MediaServer.WebRtc.Common;
using MediaServer.WebRtc.Managed;
using System;
using System.Collections.Generic;
using System.Threading;

namespace MediaServer.Core.Models
{
    sealed class PeerConnectionAdapter : IPeerConnection
    {
        readonly PeerConnectionObserver _peerConnectionObserverImpl;
        readonly PeerConnection _peerConnectionImpl;
        readonly object _syncRoot = new object();
        readonly VideoRouter _videoRouter;
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

        PeerConnection IPeerConnection.Native => _peerConnectionImpl;

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

        public void SetRemoteSessionDescription(RTCSessionDescription description, Callback callback)
        {
            if(callback is null)
                throw new ArgumentNullException(nameof(callback));
            MustNotDisposed();
            _peerConnectionImpl.SetRemoteSessionDescription(
                description.Type,
                description.Sdp,
                new Callback().OnSuccess(delegate
                {
                    try
                    {
                        // TODO: move this,
                        // AddPeerConnection() should be called right after the peer connection
                        // factory returns the newly created PeerConnection.
                        //
                        // To accomplish this, make sure VideoRouter listens to PeerConnection's track events,
                        // because when PeerConnection has just been created, it has no track, however 
                        // SetRemoteSessionDescription() above adds track after the PeerConnection is added to the router.
                        if(Interlocked.CompareExchange(
                            ref _addedToRouterState,
                            (int)AddedToRouterState.Added,
                            (int)AddedToRouterState.NotAdded) == (int)AddedToRouterState.NotAdded)
                        {
                            // As per webRTC example, the answerer will SetRemoteSessionDescription() first,
                            // then followed by AddTrack();
                            //
                            // and AddPeerConnectionAsync() will call AddTrack() under the hood,
                            // therefore we call AddPeerConnectionAsync() right after SetRemoteSessionDescription();
                            _videoRouter.AddPeerConnection(Device, this);
                        }
                        callback.Success();
                    }
                    catch(Exception ex)
                    {
                        callback.Error(ex.Message);
                    }

                }).OnError(msg => callback.Error(msg)));
        }

        public void CreateOffer(Callback<RTCSessionDescription> callback)
        {
            if(callback is null)
                throw new ArgumentNullException(nameof(callback));
            MustNotDisposed();
            LastOfferId = Guid.NewGuid();
            _peerConnectionImpl.CreateOffer(callback);
        }

        public void CreateAnswer(Callback<RTCSessionDescription> callback)
        {
            MustNotDisposed();
            _peerConnectionImpl.CreateAnswer(callback);
        }

        public void SetLocalSessionDescription(RTCSessionDescription localDescription, Callback callback)
        {
            MustNotDisposed();
            // After generating answer, must set LocalSdp,
            // otherwise ICE candidates won't be gathered.
            _peerConnectionImpl.SetLocalSessionDescription(
                localDescription.Type,
                localDescription.Sdp,
                callback);
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
                _videoRouter.RemovePeerConnection(Device, this);
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

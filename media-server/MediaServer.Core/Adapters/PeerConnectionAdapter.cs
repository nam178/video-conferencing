using MediaServer.Common.Utils;
using MediaServer.Core.Models;
using MediaServer.Models;
using MediaServer.WebRtc.Managed;
using MediaServer.WebRtc.MediaRouting;
using NLog;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MediaServer.Core.Adapters
{
    sealed class PeerConnectionAdapter : IPeerConnection
    {
        readonly PeerConnectionObserver _peerConnectionObserverImpl;
        readonly PeerConnection _peerConnectionImpl;
        readonly object _syncRoot = new object();
        readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        readonly VideoRouter _videoRouter;

        Action<IPeerConnection, RTCIceCandidate> _iceCandidateObserver;
        Action<IPeerConnection> _renegotationNeededObserver;

        public IRoom Room { get; }

        public IRemoteDevice Device { get; }

        public Guid Id => _peerConnectionImpl.Id;

        public PeerConnectionAdapter(
            IRoom room,
            IRemoteDevice device,
            PeerConnectionFactory peerConnectionFactory,
            IReadOnlyList<string> stunUrls,
            VideoRouter videoRouter)
        {
            if(peerConnectionFactory is null)
                throw new ArgumentNullException(nameof(peerConnectionFactory));
            if(stunUrls is null)
                throw new ArgumentNullException(nameof(stunUrls));
            Room = room ?? throw new ArgumentNullException(nameof(room));
            Device = device ?? throw new ArgumentNullException(nameof(device));
            _videoRouter = videoRouter ?? throw new ArgumentNullException(nameof(videoRouter));
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

        int _mediaRoutingStatus;
        public async Task StartMediaRoutingAsync()
        {
            MustNotDisposed();
            if(Interlocked.CompareExchange(ref _mediaRoutingStatus, 1, 0) != 0)
            {
                throw new InvalidOperationException();
            }
            await _videoRouter.AddPeerConnectionAsync(Device.Id, _peerConnectionImpl, _peerConnectionObserverImpl);
        }

        public Task SetRemoteSessionDescriptionAsync(RTCSessionDescription description)
        {
            MustNotDisposed();
            return _peerConnectionImpl.SetRemoteSessionDescriptionAsync(description.Type, description.Sdp);
        }

        public async Task<RTCSessionDescription> CreateOfferAsync()
        {
            MustNotDisposed();
            return await _peerConnectionImpl.CreateOfferAsync();
        }

        public async Task<RTCSessionDescription> CreateAnswerAsync()
        {
            MustNotDisposed();
            return await _peerConnectionImpl.CreateAnswerAsync();
        }

        public async Task SetLocalSessionDescriptionAsync(RTCSessionDescription localDescription)
        {
            MustNotDisposed();
            // After generating answer, must set LocalSdp,
            // otherwise ICE candidates won't be gathered.
            await _peerConnectionImpl.SetLocalSessionDescriptionAsync(
                localDescription.Type,
                localDescription.Sdp);
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

        public async Task CloseAsync()
        {
            MustNotDisposed();
            await _videoRouter.RemovePeerConnectionAsync(Device.Id, _peerConnectionImpl, _peerConnectionObserverImpl);
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

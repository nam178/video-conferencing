﻿using MediaServer.Common.Utils;
using MediaServer.WebRtc.Managed;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MediaServer.Core.Models
{
    sealed class PeerConnectionAdapter : IPeerConnection
    {
        readonly PeerConnectionObserver _nativeObserver;
        readonly PeerConnection _nativePeerConnection;
        readonly Guid _id = Guid.NewGuid();
        readonly object _syncRoot = new object();

        Action<RTCIceCandidate> _iceCandidateObserver;

        public PeerConnectionAdapter(
            PeerConnectionFactory webRtcPeerConnectionFactory,
            IReadOnlyList<string> stunUrls)
        {
            if(webRtcPeerConnectionFactory is null)
                throw new System.ArgumentNullException(nameof(webRtcPeerConnectionFactory));
            if(stunUrls is null)
                throw new System.ArgumentNullException(nameof(stunUrls));

            var iceServerInfo = new PeerConnectionConfig.IceServerInfo();
            foreach(var url in stunUrls)
            {
                iceServerInfo.Urls.Add(url);
            }
            var config = new PeerConnectionConfig();
            config.IceServers.Add(iceServerInfo);
            _nativeObserver = new PeerConnectionObserver();
            _nativeObserver.IceCandidateAdded += IceCandidateAdded;
            _nativePeerConnection = webRtcPeerConnectionFactory.CreatePeerConnection(_nativeObserver, config);
        }

        public Task SetRemoteSessionDescriptionAsync(RTCSessionDescription description)
            => _nativePeerConnection.SetRemoteSessionDescriptionAsync(description.Type, description.Sdp);

        public async Task<RTCSessionDescription> CreateAnswerAsync()
        {
            var localDescription = await _nativePeerConnection.CreateAnswerAsync();
            // After generating answer, must set LocalSdp,
            // otherwise ICE candidates won't be gathered.
            await _nativePeerConnection.SetLocalSessionDescriptionAsync(
                localDescription.Type,
                localDescription.Sdp);
            return localDescription;
        }

        public void AddIceCandidate(RTCIceCandidate iceCandidate)
            => _nativePeerConnection.AddIceCandidate(iceCandidate);

        public void ObserveIceCandidate(Action<RTCIceCandidate> observer) => _iceCandidateObserver = observer;

        void IceCandidateAdded(object sender, EventArgs<RTCIceCandidate> e) => _iceCandidateObserver?.Invoke(e.Payload);

        int _disposed;
        public void Dispose()
        {
            if(Interlocked.CompareExchange(ref _disposed, 1, 0) == 0)
            {
                // Order matters! PeerConnection must be closed first;
                _nativePeerConnection.Close();
                _nativePeerConnection.Dispose();
                // Observer later, cuz PeerConnection uses it
                _nativeObserver.IceCandidateAdded -= IceCandidateAdded;
                _nativeObserver.Dispose();
            }
        }

        public override string ToString() => $"[PeerConnectionAdapter Id={_id.ToString().Substring(0, 8)}]";
    }
}

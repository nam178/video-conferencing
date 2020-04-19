﻿using MediaServer.WebRtc.Managed;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MediaServer.Core.Models
{
    sealed class PeerConnectionAdapter : IPeerConnection
    {
        readonly PeerConnectionObserver _nativeObserver;
        readonly WebRtc.Managed.PeerConnection _nativePeerConnection;
        readonly Guid _id = Guid.NewGuid();

        public PeerConnectionAdapter(
            WebRtc.Managed.PeerConnectionFactory webRtcPeerConnectionFactory,
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
            _nativePeerConnection = webRtcPeerConnectionFactory.CreatePeerConnection(_nativeObserver, config);
        }

        public Task SetRemoteSessionDescriptionAsync(RTCSessionDescription description)
        {
            return _nativePeerConnection.SetRemoteSessionDescriptionAsync(description.Type, description.Sdp);
        }

        public Task<RTCSessionDescription> CreateAnswerAsync()
        {
            return _nativePeerConnection.CreateAnswerAsync();
        }

        public void AddIceCandidate(RTCIceCandidate iceCandidate)
        {
            _nativePeerConnection.AddIceCandidate(iceCandidate);
        }

        int _disposed;
        public void Dispose()
        {
            if(Interlocked.CompareExchange(ref _disposed, 1, 0) == 0)
            {
                // Order matters! PeerConnection must be closed first;
                _nativePeerConnection.Close();
                _nativePeerConnection.Dispose();
                // Observer later, cuz PeerConnection uses it
                _nativeObserver.Dispose();
            }
        }

        public override string ToString() => $"[PeerConnectionAdapter Id={_id.ToString().Substring(0, 8)}]";
    }
}

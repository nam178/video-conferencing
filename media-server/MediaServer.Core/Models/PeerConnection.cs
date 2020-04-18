using MediaServer.WebRtc.Managed;
using NLog;
using System.Collections.Generic;

namespace MediaServer.Core.Models
{
    sealed class PeerConnection : IPeerConnection
    {
        readonly PeerConnectionObserver _nativeObserver;
        readonly WebRtc.Managed.PeerConnection _nativePeerConnection;
        readonly ILogger _logger = NLog.LogManager.GetCurrentClassLogger();

        public RTCSessionDescription RemoteSessionDescription { get; set; }

        public PeerConnection(
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

        public void Dispose()
        {
            // Order matters! PeerConnection must be closed first;
            _nativePeerConnection.Close();
            _nativePeerConnection.Dispose();
            _nativeObserver.Dispose();
        }
    }
}

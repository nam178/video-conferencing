using MediaServer.Core.Common;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading;

namespace MediaServer.Core.Models
{
    sealed class PeerConnectionFactoryAdapter : IPeerConnectionFactory
    {
        readonly WebRtc.Managed.PeerConnectionFactory _webRtcPeerConnectionFactory;
        readonly IOptions<PeerConnectionFactorySettings> _peerConnectionFactorySettings;

        public PeerConnectionFactoryAdapter(IOptions<PeerConnectionFactorySettings> peerConnectionFactorySettings)
        {
            if(_peerConnectionFactorySettings.Value.StunUrls is null
                || !_peerConnectionFactorySettings.Value.StunUrls.Any())
            {
                throw new ArgumentException("StunUrls cannot be null or empty");
            }

            // actual implementation uses WebRTC
            _webRtcPeerConnectionFactory = new WebRtc.Managed.PeerConnectionFactory();
            _peerConnectionFactorySettings = peerConnectionFactorySettings
                ?? throw new System.ArgumentNullException(nameof(peerConnectionFactorySettings));
        }

        int _initialised = 0;

        public void EnsureInitialised()
        {
            if(Interlocked.CompareExchange(ref _initialised, 1, 0) == 0)
            {
                _webRtcPeerConnectionFactory.Initialize();
            }
        }

        public IPeerConnection Create()
        {
            return new PeerConnectionAdapter(_webRtcPeerConnectionFactory, _peerConnectionFactorySettings.Value.StunUrls);
        }
    }
}

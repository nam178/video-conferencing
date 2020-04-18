using System;
using System.Threading;

namespace MediaServer.Core.Models
{
    sealed class PeerConnectionFactoryAdapter : IPeerConnectionFactory
    {
        readonly WebRtc.Managed.PeerConnectionFactory _webRtcPeerConnectionFactory;

        public PeerConnectionFactoryAdapter()
        {
            // actual implementation uses WebRTC
            _webRtcPeerConnectionFactory = new WebRtc.Managed.PeerConnectionFactory();
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
            var stunUrls = Environment.GetEnvironmentVariable("STUN_URLS");
            if(string.IsNullOrWhiteSpace(stunUrls))
            {
                throw new ApplicationException("STUN_URLS environment variable has not been set");
            }
            return new PeerConnectionAdapter(_webRtcPeerConnectionFactory, new[] {
                stunUrls
            });
        }
    }
}

using MediaServer.Models;
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

        public IPeerConnection Create(IRemoteDevice remoteDevice, IRoom room)
        {
            if(remoteDevice is null)
                throw new ArgumentNullException(nameof(remoteDevice));
            if(room is null)
                throw new ArgumentNullException(nameof(room));

            var stunUrls = Environment.GetEnvironmentVariable("STUN_URLS");
            if(string.IsNullOrWhiteSpace(stunUrls))
            {
                throw new ApplicationException("STUN_URLS environment variable has not been set");
            }
            return new PeerConnectionAdapter(
                room,
                remoteDevice,
                _webRtcPeerConnectionFactory, 
                new[] {
                    stunUrls
                });
        }
    }
}

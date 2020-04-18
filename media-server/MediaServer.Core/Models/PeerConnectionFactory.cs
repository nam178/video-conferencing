using System.Threading;

namespace MediaServer.Core.Models
{
    sealed class PeerConnectionFactory : IPeerConnectionFactory
    {
        readonly WebRtc.Managed.PeerConnectionFactory _webRtcPeerConnectionFactory;

        public PeerConnectionFactory()
        {
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
            return null;
        }
    }
}

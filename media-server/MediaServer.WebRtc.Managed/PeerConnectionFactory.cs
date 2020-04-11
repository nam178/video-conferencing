using System;
using System.Linq;

namespace MediaServer.WebRtc.Managed
{
    public sealed class PeerConnectionFactory : IDisposable
    {
        PeerConnectionFactorySafeHandle Native { get; }

        public PeerConnectionFactory()
        {
            Native = new PeerConnectionFactorySafeHandle();
        }

        public PeerConnection2 CreatePeerConnection(PeerConnectionObserver observer, PeerConnectionConfig config)
        {
            var interopIceServers = config.IceServers.Select(s => new PeerConnectionFactoryInterops.IceServerConfig
            {
                Username = s.Username,
                Password = s.Password,
                CommaSeperatedUrls = string.Join(';', config.IceServers)
            }).ToArray();
            return new PeerConnection2(PeerConnectionFactoryInterops.CreatePeerConnection(
                Native,
                interopIceServers,
                interopIceServers.Count(),
                observer.Native
                ));
        }

        public void Initialize() => PeerConnectionFactoryInterops.Initialize(Native);

        public void TearDown() => PeerConnectionFactoryInterops.TearDown(Native);

        public void Dispose() => Native.Dispose();
    }
}

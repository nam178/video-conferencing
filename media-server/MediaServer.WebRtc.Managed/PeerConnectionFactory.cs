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

        public PeerConnection CreatePeerConnection(PeerConnectionObserver observer, PeerConnectionConfig config)
        {
            if(observer is null)
                throw new ArgumentNullException(nameof(observer));
            if(config is null)
                throw new ArgumentNullException(nameof(config));

            var interopIceServers = config.IceServers.Select(s => new PeerConnectionFactoryInterop.IceServerConfig
            {
                Username = s.Username,
                Password = s.Password,
                CommaSeperatedUrls = string.Join(';', config.IceServers)
            }).ToArray();
            return new PeerConnection(PeerConnectionFactoryInterop.CreatePeerConnection(
                Native,
                interopIceServers,
                interopIceServers.Count(),
                observer.Native
                ));
        }

        public VideoTrack CreatePassiveVideoTrack(string videoTrackName, PassiveVideoTrackSource source)
        {
            return new VideoTrack(PeerConnectionFactoryInterop.CreateVideoTrack(Native, source.Native, videoTrackName));
        }

        public void Initialize() => PeerConnectionFactoryInterop.Initialize(Native);

        public void TearDown() => PeerConnectionFactoryInterop.TearDown(Native);

        public void Dispose() => Native.Dispose();
    }
}

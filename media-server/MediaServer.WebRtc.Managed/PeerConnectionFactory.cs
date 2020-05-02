using System;
using System.Linq;
using System.Threading;

namespace MediaServer.WebRtc.Managed
{
    public sealed class PeerConnectionFactory : IDisposable
    {
        int _initialisationState = 0;

        internal PeerConnectionFactorySafeHandle Handle { get; }

        public RtcThread SignallingThread { get; private set; }

        public PeerConnectionFactory()
        {
            Handle = new PeerConnectionFactorySafeHandle();
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
                Handle,
                interopIceServers,
                interopIceServers.Count(),
                observer.Native
                ));
        }

        public void Initialize()
        {
            if(Interlocked.CompareExchange(ref _initialisationState, 1, 0) >= 1)
            {
                throw new InvalidOperationException();
            }
            PeerConnectionFactoryInterop.Initialize(Handle);
            SignallingThread = new RtcThread(PeerConnectionFactoryInterop.GetSignallingThread(Handle));
        }

        /// <summary>
        /// Create a video track that gets frame from provided source
        /// </summary>
        /// <param name="videoTrackName"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        /// <remarks>This can be called on any thread</remarks>
        public VideoTrack CreateVideoTrack(string videoTrackName, PassiveVideoTrackSource source)
        {
            RequireInitialised();
            return new VideoTrack(PeerConnectionFactoryInterop.CreateVideoTrack(Handle, source.Handle, videoTrackName));
        }

        public void TearDown()
        {
            if(Interlocked.CompareExchange(ref _initialisationState, 2, 1) == 1)
            {
                PeerConnectionFactoryInterop.TearDown(Handle);
            }
        }

        public void Dispose() => Handle.Dispose();

        void RequireInitialised()
        {
            if(Interlocked.CompareExchange(ref _initialisationState, 0, 0) != 1)
            {
                throw new InvalidOperationException();
            }
        }
    }
}

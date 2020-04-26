using System;
using System.Linq;

namespace MediaServer.WebRtc.Managed
{
    public sealed class PeerConnectionFactory : IDisposable
    {
        internal PeerConnectionFactorySafeHandle Handle { get; }

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

        /// <summary>
        /// Create a video track that gets frame from provided source
        /// </summary>
        /// <param name="videoTrackName"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        /// <remarks>This can be called on any thread</remarks>
        public VideoTrack CreateVideoTrack(string videoTrackName, PassiveVideoTrackSource source)
            => new VideoTrack(PeerConnectionFactoryInterop.CreateVideoTrack(Handle, source.Handle, videoTrackName));

        /// <summary>
        /// Create a VideoSink that push frames into the provided source
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        /// <remarks>This can be called on any thread, but part of it executes on worker thread (when adding sink -> source)</remarks>
        public VideoSinkAdapter CreateVideoSinkAdapter(PassiveVideoTrackSource target)
            => new VideoSinkAdapter(this, target);

        public void Initialize() => PeerConnectionFactoryInterop.Initialize(Handle);

        public void TearDown() => PeerConnectionFactoryInterop.TearDown(Handle);

        public void Dispose() => Handle.Dispose();
    }
}

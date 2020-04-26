using System;

namespace MediaServer.WebRtc.Managed
{
    public sealed class VideoSinkAdapter : IDisposable
    {
        internal VideoSinkAdapterSafeHandle Handle { get; }

        internal VideoSinkAdapter(PeerConnectionFactory peerConnectionFactory, PassiveVideoTrackSource trackSource)
        {
            Handle = new VideoSinkAdapterSafeHandle(peerConnectionFactory.Handle, trackSource.Handle);
        }

        public void Dispose() => Handle.Dispose();
    }
}

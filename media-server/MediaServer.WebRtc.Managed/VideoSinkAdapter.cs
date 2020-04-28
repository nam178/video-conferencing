using System;

namespace MediaServer.WebRtc.Managed
{
    public sealed class VideoSinkAdapter : IDisposable
    {
        readonly PassiveVideoTrackSource _trackSource;
        readonly bool _ownTrackSource;

        internal VideoSinkAdapterSafeHandle Handle { get; }

        internal VideoSinkAdapter(PeerConnectionFactory peerConnectionFactory, PassiveVideoTrackSource trackSource, bool ownTrackSource)
        {
            _trackSource = trackSource ?? throw new ArgumentNullException(nameof(trackSource));
            _ownTrackSource = ownTrackSource;
            Handle = new VideoSinkAdapterSafeHandle(peerConnectionFactory.Handle, trackSource.Handle);
        }

        public void Dispose()
        {
            if(_ownTrackSource)
            {
                _trackSource.Dispose();
            }
            Handle.Dispose();
        }
    }
}

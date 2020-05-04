using System;

namespace MediaServer.WebRtc.Managed
{
    public sealed class VideoSinkAdapter : IDisposable
    {
        readonly PassiveVideoTrackSource _trackSource;
        readonly bool _ownTrackSource;

        internal VideoSinkAdapterSafeHandle Handle { get; }

        public VideoSinkAdapter(PassiveVideoTrackSource trackSource, bool ownTrackSource)
        {
            _trackSource = trackSource ?? throw new ArgumentNullException(nameof(trackSource));
            _ownTrackSource = ownTrackSource;
            Handle = new VideoSinkAdapterSafeHandle(trackSource.Handle);
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

using System;

namespace MediaServer.WebRtc.Managed
{
    public sealed class PassiveVideoTrack : IDisposable
    {
        readonly PassiveVideoTrackSource _source; // keep a ref to source so it won't disposed

        internal PassiveVideoTrackSafeHandle Native { get; }

        public PassiveVideoTrack(string videoTrackName, PassiveVideoTrackSource source)
        {
            if(videoTrackName is null)
                throw new ArgumentNullException(nameof(videoTrackName));
            if(source is null)
                throw new ArgumentNullException(nameof(source));
            Native = new PassiveVideoTrackSafeHandle(videoTrackName, source.Native);
            _source = source;
            _source.AddedToVideoTrack(this);
        }

        public void Dispose() => Native.Dispose();
    }
}

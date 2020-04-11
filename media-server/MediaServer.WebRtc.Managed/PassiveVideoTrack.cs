using System;

namespace MediaServer.WebRtc.Managed
{
    public sealed class PassiveVideoTrack : IDisposable
    {
        readonly PassiveVideoTrackSource _source; // keep a ref to source so it won't disposed

        internal PassiveVideoTrackSafeHandle Native { get; }

        public PassiveVideoTrack(IntPtr native)
        {
            Native = new PassiveVideoTrackSafeHandle(native);
            _source.AddedToVideoTrack(this);
        }

        public void Dispose() => Native.Dispose();
    }
}

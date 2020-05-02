using System;

namespace MediaServer.WebRtc.Managed
{
    public sealed class PassiveVideoTrackSource : IDisposable
    {
        internal PassiveVideoTrackSourceSafeHandle Handle { get; }

        public PassiveVideoTrackSource()
        {
            Handle = new PassiveVideoTrackSourceSafeHandle();
        }

        public void Dispose() => Handle.Dispose();
    }
}



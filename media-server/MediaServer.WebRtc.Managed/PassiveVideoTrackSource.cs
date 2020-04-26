using System;
using System.Collections.Generic;

namespace MediaServer.WebRtc.Managed
{
    public sealed class PassiveVideoTrackSource : IDisposable
    {
        internal PassiveVideoTrackSourceSafeHandle Handle { get; }

        public PassiveVideoTrackSource()
        {
            Handle = new PassiveVideoTrackSourceSafeHandle();
        }

        public void PushVideoFrame(IntPtr videoFrame) => PassiveVideoTrackSourceInterop.PushVideoFrame(Handle, videoFrame);

        public void Dispose()
        {
            Handle.Dispose();
        }
    }
}



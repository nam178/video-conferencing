using System;

namespace MediaServer.WebRtc.Managed
{
    public sealed class VideoTrack : MediaStreamTrack
    {
        public VideoTrack(IntPtr native) : base(native)
        {
        }
    }
}
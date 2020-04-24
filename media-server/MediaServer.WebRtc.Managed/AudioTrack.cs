using System;

namespace MediaServer.WebRtc.Managed
{
    public sealed class AudioTrack : MediaStreamTrack
    {
        public AudioTrack(IntPtr native) : base(native)
        {
        }
    }
}
using System;

namespace MediaServer.WebRtc.Managed
{
    public sealed class VideoTrack : MediaStreamTrack
    {
        public VideoTrack(IntPtr native) : base(native)
        {
        }

        public void AddSink(VideoSinkAdapter videoSinkAdapter)
        {
            VideoTrackInterop.AddSink(Handle, videoSinkAdapter.Handle);
        }

        public void RemoveSink(VideoSinkAdapter videoSinkAdapter)
        {
            VideoTrackInterop.RemoveSink(Handle, videoSinkAdapter.Handle);
        }
    }
}
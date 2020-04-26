using System.Runtime.InteropServices;

namespace MediaServer.WebRtc.Managed
{
    static class VideoTrackInterop
    {
        [DllImport(InteropSettings.DLL_PATH, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "VideoTrackAddSink")]
        public static extern void AddSink(MediaStreamTrackSafeHandle videoTrackSafeHandle, VideoSinkAdapterSafeHandle videoSinkAdapterSafeHandle);

        [DllImport(InteropSettings.DLL_PATH, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "VideoTrackRemoveSink")]
        public static extern void RemoveSink(MediaStreamTrackSafeHandle videoTrackSafeHandle, VideoSinkAdapterSafeHandle videoSinkAdapterSafeHandle);
    }
}
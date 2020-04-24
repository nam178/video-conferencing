using System;
using System.Runtime.InteropServices;

namespace MediaServer.WebRtc.Managed
{
    static class MediaStreamTrackInterop
    {
        [DllImport(InteropSettings.DLL_PATH, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "MediaStreamTrackDestroy")]
        public static extern void Destroy(IntPtr mediaStreamTrackPtr);

        [DllImport(InteropSettings.DLL_PATH, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "MediaStreamTrackIsAudioTrack")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool IsAudioTrack(IntPtr mediaStreamTrackPtr);

        [DllImport(InteropSettings.DLL_PATH, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "MediaStreamTrackIsAudioTrack")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool IsAudioTrack(MediaStreamTrackSafeHandle mediaStreamTrackPtr);

        [DllImport(InteropSettings.DLL_PATH, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "MediaStreamTrackId")]
        public static extern IntPtr Id(MediaStreamTrackSafeHandle hande);
    }
}
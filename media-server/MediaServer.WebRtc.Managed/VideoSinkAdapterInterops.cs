using System;
using System.Runtime.InteropServices;

namespace MediaServer.WebRtc.Managed
{
    static class VideoSinkAdapterInterops
    {
        [DllImport(InteropSettings.DLL_PATH, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "VideoSinkAdapterCreate")]
        public static extern IntPtr Create(PeerConnectionFactorySafeHandle peerConnectionFactory, PassiveVideoTrackSourceSafeHandle passiveVideoTrackSource);

        [DllImport(InteropSettings.DLL_PATH, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "VideoSinkAdapterDestroy")]
        public static extern void Destroy(IntPtr videoSinkAdapterHandle);
    }
}

using System;
using System.Runtime.InteropServices;

namespace MediaServer.WebRtc.Managed
{
    static class PassiveVideoTrackInterop
    {
        [DllImport(Utils.DLL_PATH, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "PassiveVideoTrackCreate")]
        public static extern IntPtr Create(string videoTrackName, PassiveVideoTrackSourceSafeHandle passiveVideoTrackSource);

        [DllImport(Utils.DLL_PATH, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "PassiveVideoTrackDelete")]
        public static extern void Delete(ref IntPtr passiveVideoTrackIntPtr);
    }
}

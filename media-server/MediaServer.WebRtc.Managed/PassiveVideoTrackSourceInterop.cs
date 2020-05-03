using System;
using System.Runtime.InteropServices;

namespace MediaServer.WebRtc.Managed
{
    static class PassiveVideoTrackSourceInterop
    {
        [DllImport(InteropSettings.DLL_PATH, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "PassiveVideoTrackSourceCreate")]
        public static extern IntPtr Create();

        [DllImport(InteropSettings.DLL_PATH, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "PassiveVideoTrackSourceRelease")]
        public static extern void Release(IntPtr passiveVideoTrackSource);
        
        [DllImport(InteropSettings.DLL_PATH, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "PassiveVideoTrackSourceAddRef")]
        public static extern void AddRef(IntPtr passiveVideoTrackSource);
    }
}



using System;
using System.Runtime.InteropServices;

namespace MediaServer.WebRtc.Managed
{
    static class PassiveVideoTrackInterop
    {
        [DllImport(InteropSettings.DLL_PATH, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "PassiveVideoTrackDelete")]
        public static extern void Delete(ref IntPtr passiveVideoTrackIntPtr);
    }
}

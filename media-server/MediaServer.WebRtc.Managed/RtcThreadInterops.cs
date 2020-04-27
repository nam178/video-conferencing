using System;
using System.Runtime.InteropServices;

namespace MediaServer.WebRtc.Managed
{
    static class RtcThreadInterops
    {
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public delegate void Handler(IntPtr userData);

        [DllImport(InteropSettings.DLL_PATH, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "RtcThreadPost")]
        public static extern void Post(IntPtr rtcThread, Handler handler, IntPtr userData);
    }
}
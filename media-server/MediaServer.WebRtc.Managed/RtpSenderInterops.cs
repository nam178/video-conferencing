using System;
using System.Runtime.InteropServices;

namespace MediaServer.WebRtc.Managed
{
    static class RtpSenderInterops
    {
        [DllImport(InteropSettings.DLL_PATH, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "RtpSenderDestroy")]
        public static extern void Destroy(IntPtr rtpSenderPtr);
    }
}
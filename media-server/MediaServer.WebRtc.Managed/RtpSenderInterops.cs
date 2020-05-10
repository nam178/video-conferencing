using System;
using System.Runtime.InteropServices;

namespace MediaServer.WebRtc.Managed
{
    static class RtpSenderInterops
    {
        [DllImport(InteropSettings.DLL_PATH, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "RtpSenderDestroy")]
        public static extern void Destroy(IntPtr rtpSenderPtr);

        [DllImport(InteropSettings.DLL_PATH, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "RtpSenderGetTrack")]
        public static extern IntPtr GetTrack(RtpSenderSafeHandle rptSender);

        [DllImport(InteropSettings.DLL_PATH, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "RtpSenderSetTrack")]
        public static extern void SetTrack(RtpSenderSafeHandle rptSender, IntPtr track);
    }
}
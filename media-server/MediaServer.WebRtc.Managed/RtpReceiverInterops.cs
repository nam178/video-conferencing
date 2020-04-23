using System;
using System.Runtime.InteropServices;

namespace MediaServer.WebRtc.Managed
{
    static class RtpReceiverInterops
    {
        [DllImport(InteropSettings.DLL_PATH, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "RtpReceiverDestroy")]
        public static extern void Destroy(IntPtr ptr);

        [DllImport(InteropSettings.DLL_PATH, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "RtpReceiverGetRtpReceiverInterface")]
        public static extern IntPtr GetRtpReceiverInterface(RtpReceiverSafeHandle rtpReceiverWrapper);
        
        [DllImport(InteropSettings.DLL_PATH, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "RtpReceiverGetTrack")]
        public static extern IntPtr GetTrack(RtpReceiverSafeHandle rtpReceiverWrapper);
    }
}

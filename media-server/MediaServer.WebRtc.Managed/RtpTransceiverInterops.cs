using System;
using System.Runtime.InteropServices;

namespace MediaServer.WebRtc.Managed
{
    static class RtpTransceiverInterops
    {
        [DllImport(InteropSettings.DLL_PATH, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "RtpTransceiverDestroy")]
        public static extern void Destroy(IntPtr native);

        [DllImport(InteropSettings.DLL_PATH, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "RtpTransceiverGetSender")]
        public static extern IntPtr GetSender(RtpTransceiverSafeHandle rtpTransceiver);

        [DllImport(InteropSettings.DLL_PATH, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "RtpTransceiverGetReceiver")]
        public static extern IntPtr GetReceiver(RtpTransceiverSafeHandle rtpTransceiver);
        
        [DllImport(InteropSettings.DLL_PATH, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "RtpTransceiverMid")]
        public static extern IntPtr Mid(RtpTransceiverSafeHandle rtpTransceiver);
    }
}
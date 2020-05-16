using System;
using System.Runtime.InteropServices;

namespace MediaServer.WebRtc.Managed
{
    static partial class RtpTransceiverInterops
    {
        [DllImport(InteropSettings.DLL_PATH, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "RtpTransceiverDestroy")]
        public static extern void Destroy(IntPtr native);

        [DllImport(InteropSettings.DLL_PATH, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "RtpTransceiverGetSender")]
        public static extern IntPtr GetSender(RtpTransceiverSafeHandle rtpTransceiver);

        [DllImport(InteropSettings.DLL_PATH, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "RtpTransceiverGetReceiver")]
        public static extern IntPtr GetReceiver(RtpTransceiverSafeHandle rtpTransceiver);

        [DllImport(InteropSettings.DLL_PATH, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "RtpTransceiverMid")]
        public static extern IntPtr Mid(RtpTransceiverSafeHandle rtpTransceiver);

        [DllImport(InteropSettings.DLL_PATH, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "RtpTransceiverGetMediaKind")]
        public static extern int GetMediaKind(RtpTransceiverSafeHandle rtpTransceiver);

        [DllImport(InteropSettings.DLL_PATH, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "RtpTransceiverGetDirection")]
        public static extern RtpTransceiverDirection GetDirection(RtpTransceiverSafeHandle rtpTransceiver);
        
        [DllImport(InteropSettings.DLL_PATH, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "RtpTransceiverSetDirection")]
        public static extern void SetDirection(RtpTransceiverSafeHandle rtpTransceiver, RtpTransceiverDirection direction);
        
        [DllImport(InteropSettings.DLL_PATH, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "RtpTransceiverTryGetCurrentDirection")]
        [return:MarshalAs(UnmanagedType.I1)]
        public static extern bool TryGetCurrentDirection(RtpTransceiverSafeHandle rtpTransceiver, ref RtpTransceiverDirection outDirection);
    }
}
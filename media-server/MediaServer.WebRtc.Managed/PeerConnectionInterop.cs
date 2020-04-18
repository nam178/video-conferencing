using System;
using System.Runtime.InteropServices;

namespace MediaServer.WebRtc.Managed
{
    static class PeerConnectionInterop
    {
        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Ansi)]
        public struct CreateAnswerResult
        {
            [MarshalAs(UnmanagedType.I1)]
            public bool Success;
            public string ErrorMessage;
            public string SdpType;
            public string Sdp;
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public delegate void CreateAnswerResultCallback(IntPtr userData, CreateAnswerResult result);

        [DllImport(InteropSettings.DLL_PATH, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "PeerConnectionCreateAnswer")]
        public static extern void CreateAnswer(PeerConnectionSafeHandle peerConnectionSafeHandle, CreateAnswerResultCallback callback, IntPtr userData);

        [DllImport(InteropSettings.DLL_PATH, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "PeerConnectionClose")]
        public static extern void Close(PeerConnectionSafeHandle native);
        
        [DllImport(InteropSettings.DLL_PATH, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "PeerConnectionDestroy")]
        public static extern void Destroy(IntPtr native);
    }
}

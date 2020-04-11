using System;
using System.Runtime.InteropServices;

namespace MediaServer.WebRtc.Managed
{
    static class PeerConnectionInterop
    {
        [DllImport(Utils.DLL_PATH, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "PeerConnectionDestroy")]
        public static extern void Destroy(IntPtr native);
    }
}

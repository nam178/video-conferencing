using System;
using System.Runtime.InteropServices;

namespace MediaServer.WebRtc.Managed
{
    /// <summary>
    /// Also used for interop, don't modify
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct IceCandidate
    {
        public string Sdp;
        public string SdpMid;
        public Int32 MLineIndex;
    };
}

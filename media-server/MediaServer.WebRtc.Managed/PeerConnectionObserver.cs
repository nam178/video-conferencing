using Microsoft.MixedReality.WebRTC.Interop;
using System;
using System.Runtime.InteropServices;

namespace MediaServer.WebRtc.Managed
{
    static class PeerConnectionObserverInterop
    {
        [DllImport(Utils.dllPath, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "PeerConnectionObserverCreate")]
        public extern static IntPtr Create();
        
        [DllImport(Utils.dllPath, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "PeerConnectionObserverDestroy")]
        public extern static void Destroy(IntPtr native);
    }

    sealed class PeerConnectionObserver : IDisposable
    {
        PeerConnectionObserverSafeHandle Native { get; }

        public PeerConnectionObserver()
        {
            Native = new PeerConnectionObserverSafeHandle();
        }

        public void Dispose() => Native.Dispose();
    }
}

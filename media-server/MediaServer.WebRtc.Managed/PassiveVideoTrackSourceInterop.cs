using Microsoft.MixedReality.WebRTC.Interop;
using System;
using System.Runtime.InteropServices;

namespace MediaServer.WebRtc.Managed
{
    static class PassiveVideoTrackSourceInterop
    {
        [DllImport(Utils.dllPath, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "PassiveVideoTrackSourceCreate")]
        public static extern IntPtr Create();

        [DllImport(Utils.dllPath, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "PassiveVideoTrackSourceDestroy")]
        public static extern void Destroy(ref IntPtr passiveVideoTrackSourceRef);

        [DllImport(Utils.dllPath, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "PassiveVideoTrackSourcePushVideoFrame")]
        public static extern void PushVideoFrame(PassiveVideoTrackSourceSafeHandle passiveVideoTrackSource, IntPtr videoFrame);
    }
}



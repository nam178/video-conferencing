using Microsoft.MixedReality.WebRTC.Interop;
using System;
using System.Runtime.InteropServices;

namespace Microsoft.MixedReality.WebRTC
{
    static class PassiveVideoTrackInterop
    {
        [DllImport(Utils.dllPath, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi,EntryPoint = "PassiveVideoTrackCreate")]
        public static extern IntPtr Create(string videoTrackName, PassiveVideoTrackSourceSafeHandle passiveVideoTrackSource);

        [DllImport(Utils.dllPath, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "PassiveVideoTrackDelete")]
        public static extern void Delete(ref IntPtr passiveVideoTrackIntPtr);
    }
}

﻿using System;
using System.Runtime.InteropServices;

namespace MediaServer.WebRtc.Managed
{
    static class PassiveVideoTrackSourceInterop
    {
        [DllImport(InteropSettings.DLL_PATH, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "PassiveVideoTrackSourceCreate")]
        public static extern IntPtr Create();

        [DllImport(InteropSettings.DLL_PATH, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "PassiveVideoTrackSourceDestroy")]
        public static extern void Destroy(IntPtr passiveVideoTrackSourceRef);

        [DllImport(InteropSettings.DLL_PATH, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "PassiveVideoTrackSourcePushVideoFrame")]
        public static extern void PushVideoFrame(PassiveVideoTrackSourceSafeHandle passiveVideoTrackSource, IntPtr videoFrame);
    }
}



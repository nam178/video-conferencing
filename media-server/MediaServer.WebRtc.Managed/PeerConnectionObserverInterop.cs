﻿using System;
using System.Runtime.InteropServices;

namespace MediaServer.WebRtc.Managed
{
    static class PeerConnectionObserverInterop
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct IceCandidate
        {
            public string Sdp;
            public string SdpMid;
            public Int32 MLineIndex;
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct PeerConnectionObserverCallbacks
        {
            public RenegotiationNeededCallback RenegotiationNeededCallback;
            public IceGatheringStateChangedCallback IceGatheringStateChangedCallback;
            public IceConnectionChangeCallback IceConnectionChangeCallback;
            public IceCandidateCallback IceCandidateCallback;
            public IceCandidatesRemovedCallback IceCandidatesRemovedCallback;
            public RemoteTrackAddedCallback RemoteTrackAddedCallback;
            public RemoteTrackRemovedCallback RemoteTrackRemovedCallback;
        };

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public delegate void RenegotiationNeededCallback(IntPtr userData);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public delegate void IceGatheringStateChangedCallback(IntPtr userData, RTCIceGatheringState state);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public delegate void IceConnectionChangeCallback(IntPtr userData, RTCIceConnectionState state);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public delegate void IceCandidateCallback(IntPtr userData, IceCandidate iceCandidate);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public delegate void IceCandidatesRemovedCallback(IntPtr userData, IntPtr candidates);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public delegate void RemoteTrackAddedCallback(IntPtr userData, IntPtr rtpTransceiverInterfacePtr);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public delegate void RemoteTrackRemovedCallback(IntPtr userData, IntPtr rtpReceiverInterfacePtr);

        [DllImport(InteropSettings.DLL_PATH, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "PeerConnectionObserverCreate")]
        public extern static IntPtr Create();

        [DllImport(InteropSettings.DLL_PATH, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "PeerConnectionObserverDestroy")]
        public extern static void Destroy(IntPtr native);

        [DllImport(InteropSettings.DLL_PATH, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "PeerConnectionObserverSetCallbacks")]
        public extern static void SetCallbacks(PeerConnectionObserverSafeHandle safeHandle, IntPtr userData, PeerConnectionObserverCallbacks callbacks);
    }
}

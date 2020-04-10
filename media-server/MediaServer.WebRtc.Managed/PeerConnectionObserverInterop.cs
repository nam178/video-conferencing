using Microsoft.MixedReality.WebRTC.Interop;
using System;
using System.Runtime.InteropServices;

namespace MediaServer.WebRtc.Managed
{
    static class PeerConnectionObserverInterop
    {
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public delegate void RenegotiationNeededCallback(IntPtr userData);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public delegate void IceGatheringStateChangedCallback(IntPtr userData, IceGatheringState state);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public delegate void IceConnectionChangeCallback(IntPtr userData, IceConnectionState state);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public delegate void IceCandidateCallback(IntPtr userData, IceCandidate iceCandidate);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public delegate void IceCandidatesRemovedCallback(IntPtr userData, IntPtr candidates);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public delegate void RemoteTrackAddedCallback(IntPtr userData, IntPtr rtpTransceiverInterfacePtr);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public delegate void RemoteTrackRemovedCallback(IntPtr userData, IntPtr rtpReceiverInterfacePtr);

        public enum IceGatheringState : Int32
        {
            New = 0,
            Gathering = 1,
            Complete = 2,
        };

        public enum IceConnectionState : Int32
        {
            IceConnectionNew = 0,
            IceConnectionChecking = 1,
            IceConnectionConnected = 2,
            IceConnectionCompleted = 3,
            IceConnectionFailed = 4,
            IceConnectionDisconnected = 5,
            IceConnectionClosed = 6,
            IceConnectionMax = 7
        };

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

        [DllImport(Utils.dllPath, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "PeerConnectionObserverCreate")]
        public extern static IntPtr Create();

        [DllImport(Utils.dllPath, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "PeerConnectionObserverDestroy")]
        public extern static void Destroy(IntPtr native);

        [DllImport(Utils.dllPath, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "PeerConnectionObserverSetCallbacks")]
        public extern static void SetCallbacks(PeerConnectionObserverSafeHandle safeHandle, IntPtr userData, PeerConnectionObserverCallbacks callbacks);
    }
}

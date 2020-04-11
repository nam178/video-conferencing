using System;
using System.Runtime.InteropServices;

namespace MediaServer.WebRtc.Managed
{
    static partial class PeerConnectionObserverInterop
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

        [DllImport(Utils.DLL_PATH, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "PeerConnectionObserverCreate")]
        public extern static IntPtr Create();

        [DllImport(Utils.DLL_PATH, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "PeerConnectionObserverDestroy")]
        public extern static void Destroy(IntPtr native);

        [DllImport(Utils.DLL_PATH, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "PeerConnectionObserverSetCallbacks")]
        public extern static void SetCallbacks(PeerConnectionObserverSafeHandle safeHandle, IntPtr userData, PeerConnectionObserverCallbacks callbacks);
    }
}

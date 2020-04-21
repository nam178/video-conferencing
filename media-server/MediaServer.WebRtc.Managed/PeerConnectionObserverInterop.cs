using System;
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
        public delegate void RemoteTrackAddedCallback(IntPtr userData, IntPtr rtpReceiverWrapperPtr);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public delegate void RemoteTrackRemovedCallback(IntPtr userData, IntPtr rtpReceiverPtr);

        [DllImport(InteropSettings.DLL_PATH, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "PeerConnectionObserverCreate")]
        public extern static IntPtr Create();

        [DllImport(InteropSettings.DLL_PATH, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "PeerConnectionObserverDestroy")]
        public extern static void Destroy(IntPtr native);

        [DllImport(InteropSettings.DLL_PATH, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "PeerConnectionObserverSetRenegotiationNeededCallback")]
        public extern static void SetRenegotiationNeededCallback(PeerConnectionObserverSafeHandle safeHandle, RenegotiationNeededCallback callback, IntPtr userData);
        
        [DllImport(InteropSettings.DLL_PATH, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "PeerConnectionObserverSetIceGatheringStateChangedCallback")]
        public extern static void SetIceGatheringStateChangedCallback(PeerConnectionObserverSafeHandle safeHandle, IceGatheringStateChangedCallback callback, IntPtr userData);
        
        [DllImport(InteropSettings.DLL_PATH, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "PeerConnectionObserverSetIceConnectionChangeCallback")]
        public extern static void SetIceConnectionChangeCallback(PeerConnectionObserverSafeHandle safeHandle, IceConnectionChangeCallback callback, IntPtr userData);
        
        [DllImport(InteropSettings.DLL_PATH, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "PeerConnectionObserverSetIceCandidateCallback")]
        public extern static void SetIceCandidateCallback(PeerConnectionObserverSafeHandle safeHandle, IceCandidateCallback callback, IntPtr userData);
        
        [DllImport(InteropSettings.DLL_PATH, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "PeerConnectionObserverSetIceCandidatesRemovedCallback")]
        public extern static void SetIceCandidatesRemovedCallback(PeerConnectionObserverSafeHandle safeHandle, IceCandidatesRemovedCallback callback, IntPtr userData);
        
        [DllImport(InteropSettings.DLL_PATH, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "PeerConnectionObserverSetRemoteTrackAddedCallback")]
        public extern static void SetRemoteTrackAddedCallback(PeerConnectionObserverSafeHandle safeHandle, RemoteTrackAddedCallback callback, IntPtr userData);
        
        [DllImport(InteropSettings.DLL_PATH, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "PeerConnectionObserverSetRemoteTrackRemovedCallback")]
        public extern static void SetRemoteTrackRemovedCallback(PeerConnectionObserverSafeHandle safeHandle, RemoteTrackRemovedCallback callback, IntPtr userData);
    }
}

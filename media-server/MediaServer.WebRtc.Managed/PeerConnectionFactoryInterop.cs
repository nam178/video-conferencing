using System;
using System.Runtime.InteropServices;

namespace MediaServer.WebRtc.Managed
{
    static class PeerConnectionFactoryInterop
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct IceServerConfig
        {
            public string CommaSeperatedUrls;
            public string Username;
            public string Password;
        }

        [DllImport(InteropSettings.DLL_PATH, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "PeerConnectionFactoryCreate")]
        public static extern IntPtr Create();

        [DllImport(InteropSettings.DLL_PATH, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "PeerConnectionFactoryInitialize")]
        public static extern void Initialize(PeerConnectionFactorySafeHandle hanle);

        [DllImport(InteropSettings.DLL_PATH, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "PeerConnectionFactoryTearDown")]
        public static extern void TearDown(PeerConnectionFactorySafeHandle hanle);

        [DllImport(InteropSettings.DLL_PATH, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "PeerConnectionFactoryDestroy")]
        public static extern void Destroy(IntPtr hanle);

        [DllImport(InteropSettings.DLL_PATH, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "PeerConnectionFactoryCreatePeerConnection")]
        public static extern IntPtr CreatePeerConnection(
            PeerConnectionFactorySafeHandle peerConnectionFactorySafeHandle,
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)]IceServerConfig[] iceServerConfigs,
            int iceServerConfigLength,
            PeerConnectionObserverSafeHandle peerConnectionObserver
            );

        [DllImport(InteropSettings.DLL_PATH, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "PeerConnectionFactoryCreatePassiveVideoTrack")]
        public static extern IntPtr CreatePassiveVideoTrac(
            PeerConnectionFactorySafeHandle peerConnectionFactorySafeHandle,
            PassiveVideoTrackSourceSafeHandle passiveVideoTrackSourceSafeHandle,
            string trackname
            );
    }
}

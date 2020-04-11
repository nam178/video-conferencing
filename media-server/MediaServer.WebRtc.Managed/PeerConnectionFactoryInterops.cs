using System;
using System.Runtime.InteropServices;

namespace MediaServer.WebRtc.Managed
{
    static class PeerConnectionFactoryInterops
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct IceServerConfig
        {
            public string CommaSeperatedUrls;
            public string Username;
            public string Password;
        }

        [DllImport(Utils.DLL_PATH, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "PeerConnectionFactoryManagerCreate")]
        public static extern IntPtr Create();

        [DllImport(Utils.DLL_PATH, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "PeerConnectionFactoryManagerInitialize")]
        public static extern void Initialize(PeerConnectionFactorySafeHandle hanle);

        [DllImport(Utils.DLL_PATH, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "PeerConnectionFactoryManagerTearDown")]
        public static extern void TearDown(PeerConnectionFactorySafeHandle hanle);

        [DllImport(Utils.DLL_PATH, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "PeerConnectionFactoryManagerDestroy")]
        public static extern void Destroy(IntPtr hanle);

        [DllImport(Utils.DLL_PATH, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "PeerConnectionFactoryCreatePeerConnection")]
        public static extern IntPtr CreatePeerConnection(
            PeerConnectionFactorySafeHandle peerConnectionFactoryManagerSafeHandle,
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)]IceServerConfig[] iceServerConfigs,
            int iceServerConfigLength,
            PeerConnectionObserverSafeHandle peerConnectionObserver
            );
    }
}

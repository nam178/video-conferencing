﻿using System;
using System.Runtime.InteropServices;

namespace MediaServer.WebRtc.Managed
{
    static class PeerConnectionInterop
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct CreateAnswerResult
        {
            [MarshalAs(UnmanagedType.I1)]
            public bool Success;
            public string ErrorMessage;
            public string SdpType;
            public string Sdp;
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public delegate void CreateSdpResultCallback(IntPtr userData, CreateAnswerResult result);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public delegate void SetSessionDescriptionCallback(IntPtr userData, [MarshalAs(UnmanagedType.I1)]bool success, string errorMessage);

        [DllImport(InteropSettings.DLL_PATH, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "PeerConnectionCreateAnswer")]
        public static extern void CreateAnswer(PeerConnectionSafeHandle peerConnectionSafeHandle, CreateSdpResultCallback callback, IntPtr userData);

        [DllImport(InteropSettings.DLL_PATH, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "PeerConnectionCreateOffer")]
        public static extern void CreateOffer(PeerConnectionSafeHandle peerConnectionSafeHandle, CreateSdpResultCallback callback, IntPtr userData);

        [DllImport(InteropSettings.DLL_PATH, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "PeerConnectionClose")]
        public static extern void Close(PeerConnectionSafeHandle native);

        [DllImport(InteropSettings.DLL_PATH, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "PeerConnectionDestroy")]
        public static extern void Destroy(IntPtr native);

        [DllImport(InteropSettings.DLL_PATH, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "PeerConnectionSetRemoteSessionDescription")]
        public static extern void SetRemoteSessionDescription(PeerConnectionSafeHandle handle, string type, string sdp, SetSessionDescriptionCallback callback, IntPtr userData);

        [DllImport(InteropSettings.DLL_PATH, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "PeerConnectionSetLocalSessionDescription")]
        public static extern void SetLocalSessionDescription(PeerConnectionSafeHandle handle, string type, string sdp, SetSessionDescriptionCallback callback, IntPtr userData);

        [DllImport(InteropSettings.DLL_PATH, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "PeerConnectionAddIceCandidate")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool AddIceCandidate(PeerConnectionSafeHandle handle, string sdpMid, int mLineIndex, string sdp);

        [DllImport(InteropSettings.DLL_PATH, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "PeerConnectionGetTransceivers")]
        public static extern void GetTransceivers(PeerConnectionSafeHandle handle, out IntPtr outputArray, out int outputArraySize);

        [DllImport(InteropSettings.DLL_PATH, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "PeerConnectionFreeGetTransceiversResult")]
        public static extern void FreeGetTransceiversResult(PeerConnectionSafeHandle handle, IntPtr outputArray);

        [DllImport(InteropSettings.DLL_PATH, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "PeerConnectionAddTransceiver")]
        public static extern IntPtr AddTransceiver(PeerConnectionSafeHandle handle, [MarshalAs(UnmanagedType.I1)] bool isAudioTransceiver);
    }
}

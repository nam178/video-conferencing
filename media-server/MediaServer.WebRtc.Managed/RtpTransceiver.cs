using System;
using System.Runtime.InteropServices;

namespace MediaServer.WebRtc.Managed
{

    sealed class RtpTransceiverSafeHandle : SafeHandle
    {
        public RtpTransceiverSafeHandle(IntPtr handle) : base(IntPtr.Zero, true)
        {
            SetHandle(handle);
        }

        public override bool IsInvalid => IntPtr.Zero == handle;

        protected override bool ReleaseHandle()
        {
            if(handle != IntPtr.Zero)
            {
                RtpTransceiverInterops.Destroy(handle);
            }
            return true;
        }
    }

    static class RtpTransceiverInterops
    {
        [DllImport(InteropSettings.DLL_PATH, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "RtpTransceiverDestroy")]
        public static extern void Destroy(IntPtr native);

        [DllImport(InteropSettings.DLL_PATH, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "RtpTransceiverGetSender")]
        public static extern IntPtr GetSender(RtpTransceiverSafeHandle rtpTransceiver);

        [DllImport(InteropSettings.DLL_PATH, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "RtpTransceiverGetReceiver")]
        public static extern IntPtr GetReceiver(RtpTransceiverSafeHandle rtpTransceiver);
    }

    public sealed class RtpTransceiver : IDisposable
    {
        internal RtpTransceiverSafeHandle Handle { get; }

        public RtpTransceiver(IntPtr native)
        {
            if(native == IntPtr.Zero)
            {
                throw new ArgumentException();
            }
            Handle = new RtpTransceiverSafeHandle(native);
        }

        public void Dispose() => Handle.Dispose();
    }
}
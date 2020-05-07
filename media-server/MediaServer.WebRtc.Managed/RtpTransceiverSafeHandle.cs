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
}
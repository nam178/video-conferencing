using System;
using System.Runtime.InteropServices;

namespace MediaServer.WebRtc.Managed
{
    sealed class RtpReceiverSafeHandle : SafeHandle
    {
        public RtpReceiverSafeHandle(IntPtr native) : base(IntPtr.Zero, true)
        {
            SetHandle(native);
        }

        public override bool IsInvalid => handle == IntPtr.Zero;

        protected override bool ReleaseHandle()
        {
            if(handle != IntPtr.Zero)
            {
                RtpReceiverInterops.Destroy(handle);
            }
            return true;
        }
    }
}

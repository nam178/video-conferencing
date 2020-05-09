using System;

namespace MediaServer.WebRtc.Managed
{
    sealed class RtpReceiverSafeHandle : SafeHandleBase
    {
        public RtpReceiverSafeHandle(IntPtr native) 
            : base(native)
        {
        }

        protected override void ReleaseHandle(IntPtr handle) => RtpReceiverInterops.Destroy(handle);
    }
}

using System;

namespace MediaServer.WebRtc.Managed
{
    sealed class RtpTransceiverSafeHandle : SafeHandleBase
    {
        public RtpTransceiverSafeHandle(IntPtr handle) 
            : base(handle)
        {

        }

        protected override void ReleaseHandle(IntPtr handle) => RtpTransceiverInterops.Destroy(handle);
    }
}
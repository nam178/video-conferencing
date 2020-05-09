using System;

namespace MediaServer.WebRtc.Managed
{
    sealed class RtpSenderSafeHandle : SafeHandleBase
    {
        public RtpSenderSafeHandle(IntPtr handle) : base(handle)
        {
        }

        protected override void ReleaseHandle(IntPtr handle) => RtpSenderInterops.Destroy(handle);
    }
}
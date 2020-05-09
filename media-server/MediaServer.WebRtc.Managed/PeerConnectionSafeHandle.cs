using System;

namespace MediaServer.WebRtc.Managed
{
    sealed class PeerConnectionSafeHandle : SafeHandleBase
    {
        public PeerConnectionSafeHandle(IntPtr handle) 
            : base(handle)
        {
        }

        protected override void ReleaseHandle(IntPtr handle) => PeerConnectionInterop.Destroy(handle);
    }
}

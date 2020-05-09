using System;

namespace MediaServer.WebRtc.Managed
{
    sealed class PeerConnectionObserverSafeHandle : SafeHandleBase
    {
        public PeerConnectionObserverSafeHandle()
            : base(PeerConnectionObserverInterop.Create())
        {
        }

        protected override void ReleaseHandle(IntPtr handle) => PeerConnectionObserverInterop.Destroy(handle);
    }
}
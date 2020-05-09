using System;
using System.Runtime.InteropServices;

namespace MediaServer.WebRtc.Managed
{
    sealed class PeerConnectionObserverSafeHandle : SafeHandle
    {
        public PeerConnectionObserverSafeHandle()
            : base(IntPtr.Zero, true)
        {
            SetHandle(PeerConnectionObserverInterop.Create());
        }

        public override bool IsInvalid => handle == IntPtr.Zero;

        protected override bool ReleaseHandle()
        {
            if(handle != IntPtr.Zero)
            {
                PeerConnectionObserverInterop.Destroy(handle);
            }
            return true;
        }
    }
}
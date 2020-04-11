using System;
using System.Runtime.InteropServices;

namespace MediaServer.WebRtc.Managed
{
    sealed class PeerConnectionSafeHandle : SafeHandle
    {
        public PeerConnectionSafeHandle(IntPtr handle) : base(IntPtr.Zero, true)
        {
            SetHandle(handle);
        }

        public override bool IsInvalid => handle != IntPtr.Zero;

        protected override bool ReleaseHandle()
        {
            PeerConnectionInterop.Destroy(handle);
            return true;
        }
    }
}

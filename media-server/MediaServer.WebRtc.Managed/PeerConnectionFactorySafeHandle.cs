using System;
using System.Runtime.InteropServices;

namespace MediaServer.WebRtc.Managed
{
    sealed class PeerConnectionFactorySafeHandle : SafeHandle
    {
        public PeerConnectionFactorySafeHandle()
            : base(IntPtr.Zero, true)
        {
            SetHandle(PeerConnectionFactoryInterop.Create());
        }

        public override bool IsInvalid => handle != IntPtr.Zero;

        protected override bool ReleaseHandle()
        {
            if(handle != IntPtr.Zero)
            {
                PeerConnectionFactoryInterop.Destroy(handle);
            }
            return true;
        }
    }
}
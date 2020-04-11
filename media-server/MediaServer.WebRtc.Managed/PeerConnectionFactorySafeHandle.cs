using System;
using System.Runtime.InteropServices;

namespace MediaServer.WebRtc.Managed
{
    sealed class PeerConnectionFactorySafeHandle : SafeHandle
    {
        public PeerConnectionFactorySafeHandle() 
            : base(IntPtr.Zero, true)
        {
        }

        public override bool IsInvalid => handle != IntPtr.Zero;

        protected override bool ReleaseHandle()
        {
            throw new NotImplementedException();
        }
    }
}
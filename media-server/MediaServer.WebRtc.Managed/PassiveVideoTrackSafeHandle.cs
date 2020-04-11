using System;
using System.Runtime.InteropServices;

namespace MediaServer.WebRtc.Managed
{
    sealed class PassiveVideoTrackSafeHandle : SafeHandle
    {
        public PassiveVideoTrackSafeHandle(IntPtr native)
            : base(IntPtr.Zero, true)
        {
            if(native == IntPtr.Zero)
            {
                throw new ArgumentException("Native pointer is zero");
            }
            SetHandle(native);
        }

        public override bool IsInvalid => handle == IntPtr.Zero;

        protected override bool ReleaseHandle()
        {
            PassiveVideoTrackInterop.Delete(ref handle);
            return true;
        }
    }
}

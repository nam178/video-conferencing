using System;
using System.Runtime.InteropServices;

namespace MediaServer.WebRtc.Managed
{
    sealed class PassiveVideoTrackSourceSafeHandle : SafeHandle
    {
        public PassiveVideoTrackSourceSafeHandle()
            : base(IntPtr.Zero, true)
        {
            SetHandle(PassiveVideoTrackSourceInterop.Create());
        }

        public override bool IsInvalid => handle == IntPtr.Zero;

        protected override bool ReleaseHandle()
        {
            if(handle != IntPtr.Zero)
            {
                PassiveVideoTrackSourceInterop.Destroy(handle);
            }
            return true;
        }
    }
}



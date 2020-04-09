using System;
using System.Runtime.InteropServices;

namespace Microsoft.MixedReality.WebRTC
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
            PassiveVideoTrackSourceInterop.Destroy(ref handle);
            return true;
        }
    }
}



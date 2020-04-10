using System;
using System.Runtime.InteropServices;

namespace MediaServer.WebRtc.Managed
{
    sealed class PassiveVideoTrackSafeHandle : SafeHandle
    {
        public PassiveVideoTrackSafeHandle(
            string videoTrackName,
            PassiveVideoTrackSourceSafeHandle passiveVideoTrackSource)
            : base(IntPtr.Zero, true)
        {
            SetHandle(PassiveVideoTrackInterop.Create(videoTrackName, passiveVideoTrackSource));
        }

        public override bool IsInvalid => handle == IntPtr.Zero;

        protected override bool ReleaseHandle()
        {
            PassiveVideoTrackInterop.Delete(ref handle);
            return true;
        }
    }
}

using System;

namespace MediaServer.WebRtc.Managed
{
    sealed class PassiveVideoTrackSourceSafeHandle : SafeHandleBase
    {
        public PassiveVideoTrackSourceSafeHandle()
            : base(PassiveVideoTrackSourceInterop.Create())
        {
            PassiveVideoTrackSourceInterop.AddRef(handle);
        }

        protected override void ReleaseHandle(IntPtr handle) => PassiveVideoTrackSourceInterop.Release(handle);
    }
}



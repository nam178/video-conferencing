using System;

namespace MediaServer.WebRtc.Managed
{
    sealed class MediaStreamTrackSafeHandle : SafeHandleBase
    {
        public MediaStreamTrackSafeHandle(IntPtr native) 
            : base(native)
        {
        }

        protected override void ReleaseHandle(IntPtr handle) => MediaStreamTrackInterop.Destroy(handle);
    }
}
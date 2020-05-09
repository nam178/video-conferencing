using System;

namespace MediaServer.WebRtc.Managed
{
    sealed class VideoSinkAdapterSafeHandle : SafeHandleBase
    {
        public VideoSinkAdapterSafeHandle(PassiveVideoTrackSourceSafeHandle passiveVideoTrackSourceSafeHandle)
            : base(VideoSinkAdapterInterops.Create(passiveVideoTrackSourceSafeHandle))
        {
        }

        protected override void ReleaseHandle(IntPtr handle) => VideoSinkAdapterInterops.Destroy(handle);
    }
}

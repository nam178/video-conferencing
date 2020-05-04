using System;
using System.Runtime.InteropServices;

namespace MediaServer.WebRtc.Managed
{
    sealed class VideoSinkAdapterSafeHandle : SafeHandle
    {
        public VideoSinkAdapterSafeHandle(PassiveVideoTrackSourceSafeHandle passiveVideoTrackSourceSafeHandle)
            : base(IntPtr.Zero, true)
        {
            SetHandle(VideoSinkAdapterInterops.Create(passiveVideoTrackSourceSafeHandle));
        }

        public override bool IsInvalid => handle == IntPtr.Zero;

        protected override bool ReleaseHandle()
        {
            if(handle != IntPtr.Zero)
            {
                VideoSinkAdapterInterops.Destroy(handle);
            }
            return true;
        }
    }
}

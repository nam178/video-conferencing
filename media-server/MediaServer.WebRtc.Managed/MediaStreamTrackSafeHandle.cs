using System;
using System.Runtime.InteropServices;

namespace MediaServer.WebRtc.Managed
{
    sealed class MediaStreamTrackSafeHandle : SafeHandle
    {
        public MediaStreamTrackSafeHandle(IntPtr handle) : base(IntPtr.Zero, true)
        {
            if(handle == IntPtr.Zero)
                throw new ArgumentException("Native is nullptr");
            SetHandle(handle);
        }

        public override bool IsInvalid => handle == IntPtr.Zero;

        protected override bool ReleaseHandle()
        {
            if(handle != IntPtr.Zero)
            {
                MediaStreamTrackInterop.Destroy(handle);
            }
            return true;
        }
    }
}
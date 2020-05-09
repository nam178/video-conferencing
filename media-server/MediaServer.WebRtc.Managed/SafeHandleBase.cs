using System;
using System.Runtime.InteropServices;

namespace MediaServer.WebRtc.Managed
{
    abstract class SafeHandleBase : SafeHandle
    {
        public SafeHandleBase(IntPtr native) : base(IntPtr.Zero, true)
        {
            if(native == IntPtr.Zero)
            {
                throw new ArgumentException();
            }
            SetHandle(native);
        }

        public sealed override bool IsInvalid => handle == IntPtr.Zero;

        protected sealed override bool ReleaseHandle()
        {
            if(handle != IntPtr.Zero)
            {
                ReleaseHandle(handle);
            }
            return true;
        }

        protected abstract void ReleaseHandle(IntPtr handle);
    }
}

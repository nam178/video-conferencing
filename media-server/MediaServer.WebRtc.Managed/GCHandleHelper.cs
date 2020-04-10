using System;
using System.Runtime.InteropServices;

namespace MediaServer.WebRtc.Managed
{
    static class GCHandleHelper
    {
        public static IntPtr ToIntPtr(object target) => GCHandle.ToIntPtr(GCHandle.Alloc(target, GCHandleType.Normal));

        public static IntPtr ToIntPtr(object target, out GCHandle handle)
        {
            handle = GCHandle.Alloc(target, GCHandleType.Normal);
            return GCHandle.ToIntPtr(handle);
        }

        public static T FromIntPtr<T>(IntPtr gcHandlePtr) => (T)GCHandle.FromIntPtr(gcHandlePtr).Target;
    }
}

using System;
using System.Runtime.InteropServices;

namespace MediaServer.WebRtc.Managed
{
    static class PeerConnectionInterop2
    {
        // nothing there yet for the moment
    }

    sealed class PeerConnection2SafeHandle : SafeHandle
    {
        public PeerConnection2SafeHandle(IntPtr handle) : base(IntPtr.Zero, true)
        {
            SetHandle(handle);
        }

        public override bool IsInvalid => handle != IntPtr.Zero;

        protected override bool ReleaseHandle()
        {
            throw new NotImplementedException();
        }
    }

    public class PeerConnection2 : IDisposable
    {
        readonly PeerConnection2SafeHandle _handle;

        public PeerConnection2(IntPtr unmanagedPointer)
        {
            _handle = new PeerConnection2SafeHandle(unmanagedPointer);
        }

        public void Dispose() => _handle.Dispose();
    }
}

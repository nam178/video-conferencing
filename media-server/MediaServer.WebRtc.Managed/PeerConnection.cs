using System;

namespace MediaServer.WebRtc.Managed
{
    public class PeerConnection : IDisposable
    {
        readonly PeerConnectionSafeHandle _handle;

        public PeerConnectionObserver Observer { get; }

        internal PeerConnection(IntPtr unmanagedPointer)
        {
            _handle = new PeerConnectionSafeHandle(unmanagedPointer);
        }

        public void Dispose() => _handle.Dispose();
    }
}

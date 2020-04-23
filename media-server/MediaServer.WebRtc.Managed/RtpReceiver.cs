using System;
using System.Threading;

namespace MediaServer.WebRtc.Managed
{
    public sealed class RtpReceiver : IDisposable
    {
        readonly RtpReceiverSafeHandle _native;

        public MediaStreamTrack Track { get; }

        public RtpReceiver(IntPtr native)
        {
            _native = new RtpReceiverSafeHandle(native);
            Track = new MediaStreamTrack(RtpReceiverInterops.GetTrack(_native));
        }

        internal IntPtr GetRtpReceiverInterface() => RtpReceiverInterops.GetRtpReceiverInterface(_native);

        int _disposed = 0;
        public void Dispose()
        {
            if(Interlocked.CompareExchange(ref _disposed, 1, 0) == 0)
            {
                Track.Dispose();
                _native.Dispose();
            }
        }
    }
}

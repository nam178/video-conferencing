using System;

namespace MediaServer.WebRtc.Managed
{
    public sealed class RtpReceiver : IDisposable
    {
        readonly RtpReceiverSafeHandle _native;

        public RtpReceiver(IntPtr native)
        {
            _native = new RtpReceiverSafeHandle(native);
        }

        internal IntPtr GetRtpReceiverInterface() => RtpReceiverInterops.GetRtpReceiverInterface(_native);

        public void Dispose()
        {
            _native.Dispose();
        }
    }
}

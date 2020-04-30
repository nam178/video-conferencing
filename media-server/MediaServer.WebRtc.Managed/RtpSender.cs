using System;

namespace MediaServer.WebRtc.Managed
{
    public sealed class RtpSender : IDisposable
    {
        internal RtpSenderSafeHandle Handle { get; }

        public RtpSender(IntPtr rtpSenderPtr)
        {
            Handle = new RtpSenderSafeHandle(rtpSenderPtr);
        }

        public void Dispose() => Handle.Dispose();
    }
}
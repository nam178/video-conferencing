using System;

namespace MediaServer.WebRtc.Managed
{
    public class RtpSender : IDisposable
    {
        private IntPtr _rtpSenderPtr;

        public RtpSender(IntPtr rtpSenderPtr)
        {
            _rtpSenderPtr = rtpSenderPtr;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
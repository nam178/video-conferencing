using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace MediaServer.WebRtc.Managed
{
    public sealed class RtpTransceiver : IDisposable
    {
        readonly RtpReceiver _receiver;
        readonly RtpSender _sender;

        internal RtpTransceiverSafeHandle Handle { get; }

        public RtpReceiver Receiver
        {
            get
            {
                DisposeCheck();
                return _receiver;
            }
        }

        public string Mid => Marshal.PtrToStringAnsi(RtpTransceiverInterops.Mid(Handle));

        public RtpSender Sender
        {
            get
            {
                DisposeCheck();
                return _sender;
            }
        }

        public RtpTransceiver(IntPtr native)
        {
            if(native == IntPtr.Zero)
            {
                throw new ArgumentException();
            }
            Handle = new RtpTransceiverSafeHandle(native);
            _receiver = new RtpReceiver(RtpTransceiverInterops.GetReceiver(Handle));
            _sender = new RtpSender(RtpTransceiverInterops.GetSender(Handle));
        }

        void DisposeCheck()
        {
            if(Interlocked.CompareExchange(ref _disposed, 0, 0) == 1)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

        int _disposed;
        public void Dispose()
        {
            if(Interlocked.CompareExchange(ref _disposed, 1, 0) == 0)
            {
                _receiver.Dispose();
                _sender.Dispose();
                Handle.Dispose();
            }
        }

        public override string ToString() => $"[RtpTransceiver mid={Mid}]";
    }
}
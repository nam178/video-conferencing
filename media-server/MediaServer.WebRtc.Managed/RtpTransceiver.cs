using MediaServer.Common.Media;
using MediaServer.Common.Threading;
using MediaServer.Common.Utils;
using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace MediaServer.WebRtc.Managed
{
    public enum ReusabilityState
    {
        /// <summary>
        /// The transceiver is available for use.
        /// The sender has no track and can be safely replaced with newer track.
        /// </summary>
        Available,

        /// <summary>
        /// The transceiver's sender currently has track and cannot be reused
        /// </summary>
        Busy,

        /// <summary>
        /// The transceiver is nearly availble for re-use. 
        /// Awaiting confirmation from the client to ack that is has updated its UI accordingly.
        /// 
        /// If we re-use this transceiver at this stage, may lead to video from one person showing
        /// in the wrong slot from the client side.
        /// </summary>
        Fronzen
    }

    /// <summary>
    /// </summary>
    /// <remarks>This instance is not thread safe. Access from the same thread only.</remarks>
    public sealed class RtpTransceiver : IDisposable
    {
        readonly RtpReceiver _receiver;
        readonly RtpSender _sender;
        readonly RtcThread _signallingThread;

        internal RtpTransceiverSafeHandle Handle { get; }

        public RtpReceiver Receiver
        {
            get
            {
                SafetyCheck();
                return _receiver;
            }
        }

        public string Mid
        {
            get
            {
                SafetyCheck();
                return Marshal.PtrToStringAnsi(RtpTransceiverInterops.Mid(Handle));
            }
        }

        public MediaKind MediaKind
        {
            get
            {
                SafetyCheck();
                return (MediaKind)RtpTransceiverInterops.GetMediaKind(Handle);
            }
        }

        bool _isFrozen;
        public ReusabilityState ReusabilityState
        {
            get
            {
                SafetyCheck();
                if(Sender.Track != null)
                    return ReusabilityState.Busy;
                return _isFrozen ? ReusabilityState.Fronzen : ReusabilityState.Available;
            }
        }

        public RtpSender Sender
        {
            get
            {
                SafetyCheck();
                return _sender;
            }
        }

        public RtpTransceiver(IntPtr native, RtcThread signallingThread)
        {
            if(native == IntPtr.Zero)
            {
                throw new ArgumentException();
            }
            Handle = new RtpTransceiverSafeHandle(native);
            _signallingThread = signallingThread
                ?? throw new ArgumentNullException(nameof(signallingThread));
            _receiver = new RtpReceiver(RtpTransceiverInterops.GetReceiver(Handle));
            _sender = new RtpSender(RtpTransceiverInterops.GetSender(Handle), signallingThread);
        }

        public void ToBusyState(MediaStreamTrack track, Guid streamId)
        {
            SafetyCheck();
            ThrowIfCurrentStateIsNot(ReusabilityState.Available);
            Sender.Track = track;
            Sender.StreamId = streamId.ToString();
        }

        public void ToFrozenState()
        {
            SafetyCheck();
            ThrowIfCurrentStateIsNot(ReusabilityState.Busy);
            Sender.Track = null;
            _isFrozen = true;
        }

        public void ToAvailableState()
        {
            SafetyCheck();
            ThrowIfCurrentStateIsNot(ReusabilityState.Fronzen);
            _isFrozen = false;
        }

        void ThrowIfCurrentStateIsNot(ReusabilityState desiredState)
        {
            if(ReusabilityState != desiredState)
                throw new InvalidOperationException($"Must call this method in {desiredState}, currentState={ReusabilityState}");
        }

        void SafetyCheck()
        {
            if(Interlocked.CompareExchange(ref _disposed, 0, 0) == 1)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
            _signallingThread.EnsureCurrentThread();
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
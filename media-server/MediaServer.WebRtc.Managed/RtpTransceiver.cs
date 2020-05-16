using MediaServer.Common.Media;
using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace MediaServer.WebRtc.Managed
{
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
        public TransceiverReusabilityState ReusabilityState
        {
            get
            {
                SafetyCheck();
                if(Sender.Track != null)
                    return TransceiverReusabilityState.Busy;
                return _isFrozen ? TransceiverReusabilityState.Fronzen : TransceiverReusabilityState.Available;
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

        public RtpTransceiverDirection Direction
        {
            get => RtpTransceiverInterops.GetDirection(Handle);
            private set => RtpTransceiverInterops.SetDirection(Handle, value);
        }

        public RtpTransceiverDirection? CurrentDirection
        {
            get
            {
                RtpTransceiverDirection rtpTransceiverDirection = default;
                if(RtpTransceiverInterops.TryGetCurrentDirection(Handle, ref rtpTransceiverDirection))
                {
                    return rtpTransceiverDirection;
                }
                return null;
            }
        }

        public object CustomData { get; set; }

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
            // Checks
            SafetyCheck();
            ThrowIfCurrentStateIsNot(TransceiverReusabilityState.Available);

            // Change direction - make sure the sender will send data.
            switch(Direction)
            {
                case RtpTransceiverDirection.RecvOnly:
                    Direction = RtpTransceiverDirection.SendRecv;
                    break;

                case RtpTransceiverDirection.SendOnly:
                case RtpTransceiverDirection.SendRecv:
                    // Does not have to do anything
                    break;

                case RtpTransceiverDirection.Inactive:
                case RtpTransceiverDirection.Stopped:
                    throw new InvalidProgramException(
                        $"Impossible to have {nameof(ToBusyState)} called when Direction={Direction}"
                        );
            }

            // Set track and stream
            Sender.Track = track;
            Sender.StreamId = streamId.ToString();
        }

        public void ToFrozenState()
        {
            SafetyCheck();
            ThrowIfCurrentStateIsNot(TransceiverReusabilityState.Busy);

            // Change direction - make sure the sender won't send data
            switch(Direction)
            {
                case RtpTransceiverDirection.SendRecv:
                    Direction = RtpTransceiverDirection.RecvOnly;
                    break;

                case RtpTransceiverDirection.RecvOnly:
                    // Does not have to do anything
                    break;

                case RtpTransceiverDirection.SendOnly:
                case RtpTransceiverDirection.Inactive:
                case RtpTransceiverDirection.Stopped:
                    throw new InvalidProgramException(
                        $"Impossible to have {nameof(ToFrozenState)} called when Direction={Direction}"
                        );
            }

            // Then remove track
            Sender.Track = null;
            _isFrozen = true;
        }

        public void ToAvailableState()
        {
            SafetyCheck();
            ThrowIfCurrentStateIsNot(TransceiverReusabilityState.Fronzen);
            _isFrozen = false;
        }

        void ThrowIfCurrentStateIsNot(TransceiverReusabilityState desiredState)
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

        public override string ToString() => $"[RtpTransceiver mid={Mid}, dir={Direction}, cur={CurrentDirection}]";
    }
}
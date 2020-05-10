﻿using MediaServer.Common.Threading;
using System;

namespace MediaServer.WebRtc.Managed
{
    public sealed class RtpSender : IDisposable
    {
        readonly RtcThread _signallingThread;

        internal RtpSenderSafeHandle Handle { get; }

        MediaStreamTrack _track;
        public MediaStreamTrack Track
        {
            get
            {
                _signallingThread.EnsureCurrentThread();
                return _track;
            }
            internal set
            {
                _signallingThread.EnsureCurrentThread();
                _track = value;
                RtpSenderInterops.SetTrack(Handle, _track != null ? _track.Handle.DangerousGetHandle() : IntPtr.Zero);
            }
        }

        public RtpSender(IntPtr rtpSenderPtr, RtcThread signallingThread)
        {
            Handle = new RtpSenderSafeHandle(rtpSenderPtr);
            _signallingThread = signallingThread 
                ?? throw new ArgumentNullException(nameof(signallingThread));
        }

        public void Dispose() => Handle.Dispose();

        public override string ToString() => $"[RtpSender Track={Track}]";
    }
}
﻿using System;
using System.Runtime.InteropServices;

namespace MediaServer.WebRtc.Managed
{
    sealed class MediaStreamTrackSafeHandle : SafeHandle
    {
        public MediaStreamTrackSafeHandle(IntPtr native) : base(IntPtr.Zero, true)
        {
            SetHandle(native);
        }

        public override bool IsInvalid => handle == IntPtr.Zero;

        protected override bool ReleaseHandle()
        {
            if(handle != IntPtr.Zero)
            {
                // TODO
            }
            return true;
        }
    }

    public sealed class MediaStreamTrack : IDisposable
    {
        readonly MediaStreamTrackSafeHandle _native;

        public MediaStreamTrack(IntPtr native)
        {
            if(native == IntPtr.Zero)
            {
                throw new ArgumentException(nameof(native));
            }
            _native = new MediaStreamTrackSafeHandle(native);
        }

        public void Dispose()
        {
            _native.Dispose();
        }
    }
}
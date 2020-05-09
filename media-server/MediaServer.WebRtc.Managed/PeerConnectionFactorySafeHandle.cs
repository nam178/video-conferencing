﻿using System;
using System.Runtime.InteropServices;

namespace MediaServer.WebRtc.Managed
{
    sealed class PeerConnectionFactorySafeHandle : SafeHandleBase
    {
        public PeerConnectionFactorySafeHandle()
            : base(PeerConnectionFactoryInterop.Create())
        {
        }

        protected override void ReleaseHandle(IntPtr handle) => PeerConnectionFactoryInterop.Destroy(handle);
    }
}
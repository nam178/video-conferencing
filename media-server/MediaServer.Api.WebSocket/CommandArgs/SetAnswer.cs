﻿using MediaServer.WebRtc.Managed;
using System;

namespace MediaServer.Api.WebSocket.CommandArgs
{
    sealed class SetAnswer
    {
        public RTCSessionDescription Answer { get; set; }

        public Guid PeerConnectionId { get; set; }
    }
}
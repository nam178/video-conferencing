﻿using MediaServer.WebRtc.Managed;

namespace MediaServer.WebSocket.CommandArgs
{
    public sealed class SetOffer
    {
        public RTCSessionDescription Sdp { get; set; }
    }
}
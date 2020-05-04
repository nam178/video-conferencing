using MediaServer.WebRtc.Managed;
using System;

namespace MediaServer.Api.WebSocket.CommandArgs
{
    public sealed class SetOffer
    {
        public RTCSessionDescription Offer { get; set; }

        public Guid? PeerConnectionId { get; set; }
    }
}

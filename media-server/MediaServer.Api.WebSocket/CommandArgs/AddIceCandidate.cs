using MediaServer.WebRtc.Managed;
using System;

namespace MediaServer.Api.WebSocket.CommandArgs
{
    sealed class AddIceCandidate
    {
        public RTCIceCandidate Candidate { get; set; }

        public Guid PeerConnectionId { get; set; }
    }
}

using MediaServer.WebRtc.Common;
using System;

namespace MediaServer.Api.WebSocket.CommandArgs
{
    sealed class SetAnswer
    {
        public RTCSessionDescription Answer { get; set; }

        public Guid PeerConnectionId { get; set; }

        public Guid OfferId { get; set; }
    }
}

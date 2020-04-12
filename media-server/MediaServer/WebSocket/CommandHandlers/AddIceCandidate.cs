using MediaServer.Models;
using MediaServer.WebRtc.Managed;

namespace MediaServer.WebSocket.CommandHandlers
{
    sealed class AddIceCandidate
    {
        public RTCIceCandidate Candidate { get; set; }
    }
}

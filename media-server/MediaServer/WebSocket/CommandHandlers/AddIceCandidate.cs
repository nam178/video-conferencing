using MediaServer.Models;

namespace MediaServer.WebSocket.CommandHandlers
{
    sealed class AddIceCandidate
    {
        public IceCandidate Candidate { get; set; }
    }
}

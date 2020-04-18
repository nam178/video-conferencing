using MediaServer.WebRtc.Managed;

namespace MediaServer.WebSocket.CommandArgs
{
    sealed class AddIceCandidate
    {
        public RTCIceCandidate Candidate { get; set; }
    }
}

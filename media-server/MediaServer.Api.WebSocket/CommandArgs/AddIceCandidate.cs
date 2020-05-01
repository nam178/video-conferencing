using MediaServer.WebRtc.Managed;

namespace MediaServer.Api.WebSocket.CommandArgs
{
    sealed class AddIceCandidate
    {
        public RTCIceCandidate Candidate { get; set; }
    }
}

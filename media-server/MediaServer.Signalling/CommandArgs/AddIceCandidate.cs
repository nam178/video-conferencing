using MediaServer.WebRtc.Managed;

namespace MediaServer.Signalling.CommandArgs
{
    sealed class AddIceCandidate
    {
        public RTCIceCandidate Candidate { get; set; }
    }
}

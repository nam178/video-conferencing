using MediaServer.Core.Models;
using MediaServer.WebRtc.Managed;

namespace MediaServer.Core.Services.Negotiation.MessageQueue
{
    sealed class IceCandidateMessage : Message
    {
        public RTCIceCandidate Candidate { get; }

        public IceCandidateMessage(IPeerConnection peerConnection, RTCIceCandidate candidate)
            : base(peerConnection)
        {
            Candidate = candidate;
        }
    }
}
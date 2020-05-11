using MediaServer.Core.Models;
using MediaServer.WebRtc.Managed;

namespace MediaServer.Core.Services.Negotiation
{
    public class SessionDescriptionMessage : NegotiationMessage
    {
        public RTCSessionDescription SessionDescription { get; }

        public SessionDescriptionMessage(IPeerConnection peerConnection, RTCSessionDescription sdp) :
            base(peerConnection)
        {
            SessionDescription = sdp;
        }
    }
}
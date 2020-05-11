using MediaServer.Core.Models;
using MediaServer.WebRtc.Managed;

namespace MediaServer.Core.Services.Negotiation.MessageQueue
{
    public class SdpOfferMessage : Message
    {
        public RTCSessionDescription SessionDescription { get; }

        public SdpOfferMessage(IPeerConnection peerConnection, RTCSessionDescription sdp) :
            base(peerConnection)
        {
            SessionDescription = sdp;
        }
    }
}
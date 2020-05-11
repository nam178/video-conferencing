using MediaServer.Core.Models;
using MediaServer.WebRtc.Managed;

namespace MediaServer.Core.Services.Negotiation.MessageQueue
{
    public class SdpMessage : Message
    {
        public RTCSessionDescription SessionDescription { get; }

        public SdpMessage(IPeerConnection peerConnection, RTCSessionDescription sdp) :
            base(peerConnection)
        {
            SessionDescription = sdp;
        }
    }
}
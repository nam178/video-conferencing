using MediaServer.Core.Models;

namespace MediaServer.Core.Services.Negotiation
{
    sealed class RenegotiationMessage : NegotiationMessage
    {
        public RenegotiationMessage(IPeerConnection peerConnection) 
            : base(peerConnection)
        {
        }
    }
}

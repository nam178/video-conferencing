using MediaServer.Core.Models;

namespace MediaServer.Core.Services.Negotiation.MessageQueue
{
    sealed class RenegotiationMessage : Message
    {
        public RenegotiationMessage(IPeerConnection peerConnection)
            : base(peerConnection)
        {
        }
    }
}

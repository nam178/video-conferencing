using MediaServer.Core.Models;

namespace MediaServer.Core.Services.Negotiation
{
    public class NegotiationMessage
    {
        public IPeerConnection PeerConnection { get; }

        public NegotiationMessage(IPeerConnection peerConnection)
        {
            PeerConnection = peerConnection 
                ?? throw new System.ArgumentNullException(nameof(peerConnection));
        }
    }
}
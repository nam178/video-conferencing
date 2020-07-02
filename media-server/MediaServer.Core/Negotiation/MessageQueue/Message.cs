using MediaServer.Core.Models;

namespace MediaServer.Core.Services.Negotiation.MessageQueue
{
    public class Message
    {
        public IPeerConnection PeerConnection { get; }

        public Message(IPeerConnection peerConnection)
        {
            PeerConnection = peerConnection
                ?? throw new System.ArgumentNullException(nameof(peerConnection));
        }
    }
}
using MediaServer.Common.Threading;
using MediaServer.Core.Models;
using MediaServer.WebRtc.Managed;
using System.Collections.Generic;

namespace MediaServer.Core.Services.Negotiation.MessageQueue
{
    sealed class NegotiationService : INegotiationService
    {
        readonly NegotiationQueue _negotiationQueue;

        public NegotiationService(
            IThread signallingThread,
            IEnumerable<IMessageSubscriber> subscribers)
        {
            if(signallingThread is null)
                throw new System.ArgumentNullException(nameof(signallingThread));
            if(subscribers is null)
                throw new System.ArgumentNullException(nameof(subscribers));
            _negotiationQueue = new NegotiationQueue(subscribers, signallingThread);
        }

        public void RemoteIceCandidateReceived(IPeerConnection peerConnection, RTCIceCandidate candidate)
        {
            _negotiationQueue.Enqueue(new IceCandidateMessage(peerConnection, candidate));
        }

        public void RemoteSessionDescriptionReceived(
            IPeerConnection peerConnection,
            RTCSessionDescription remoteSessionDescription)
        {
            _negotiationQueue.Enqueue(new SdpMessage(peerConnection, remoteSessionDescription));
        }

        public void RenegotiationRequired(IPeerConnection peerConnection)
        {
            _negotiationQueue.Enqueue(new RenegotiationMessage(peerConnection));
        }
    }
}

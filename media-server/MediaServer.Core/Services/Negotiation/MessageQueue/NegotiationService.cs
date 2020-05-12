using MediaServer.Common.Media;
using MediaServer.Common.Threading;
using MediaServer.Core.Models;
using MediaServer.WebRtc.Managed;
using System;
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
                throw new ArgumentNullException(nameof(signallingThread));
            if(subscribers is null)
                throw new ArgumentNullException(nameof(subscribers));
            _negotiationQueue = new NegotiationQueue(subscribers, signallingThread);
        }

        public void EnqueueLocalIceCandidate(IPeerConnection peerConnection, RTCIceCandidate candidate)
        {
            if(peerConnection is null)
                throw new ArgumentNullException(nameof(peerConnection));
            _negotiationQueue.Enqueue(new IceCandidateMessage(peerConnection, candidate, false));
        }

        public void EnqueueRemoteAnswer(IPeerConnection peerConnection, Guid offerId, RTCSessionDescription remoteSessionDescription)
        {
            if(peerConnection is null)
                throw new ArgumentNullException(nameof(peerConnection));
            _negotiationQueue.Enqueue(new SdpAnswerMessage(peerConnection, remoteSessionDescription, offerId));
        }

        public void EnqueueRemoteIceCandidate(IPeerConnection peerConnection, RTCIceCandidate candidate)
        {
            if(peerConnection is null)
                throw new ArgumentNullException(nameof(peerConnection));

            _negotiationQueue.Enqueue(new IceCandidateMessage(peerConnection, candidate, true));
        }

        public void EnqueueRemoteOffer(
            IPeerConnection peerConnection,
            RTCSessionDescription remoteSessionDescription)
        {
            if(peerConnection is null)
                throw new ArgumentNullException(nameof(peerConnection));
            _negotiationQueue.Enqueue(new SdpOfferMessage(peerConnection, remoteSessionDescription));
        }

        public void EnqueueRemoteTransceiverMetadata(
            IPeerConnection peerConnection, 
            IReadOnlyList<TransceiverMetadata> transceiverMetadataMessage)
        {
            if(peerConnection is null)
                throw new ArgumentNullException(nameof(peerConnection));
            if(transceiverMetadataMessage is null)
                throw new ArgumentNullException(nameof(transceiverMetadataMessage));
            _negotiationQueue.Enqueue(new TransceiverMetadataMessage(transceiverMetadataMessage, peerConnection));
        }

        public void EnqueueRenegotiationRequest(IPeerConnection peerConnection)
        {
            if(peerConnection is null)
                throw new ArgumentNullException(nameof(peerConnection));

            _negotiationQueue.Enqueue(new RenegotiationMessage(peerConnection));
        }
    }
}

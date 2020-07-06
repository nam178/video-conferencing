using MediaServer.Common.Media;
using MediaServer.Core.Models;
using MediaServer.WebRtc.Common;
using System;
using System.Collections.Generic;

namespace MediaServer.Core.Services.Negotiation.MessageQueue
{
    sealed class NegotiationService : INegotiationService
    {
        readonly NegotiationQueue _negotiationQueue;

        public NegotiationService(IEnumerable<IMessageSubscriber> subscribers)
        {
            if(subscribers is null)
                throw new ArgumentNullException(nameof(subscribers));
            _negotiationQueue = new NegotiationQueue(subscribers);
        }

        public void EnqueueLocalIceCandidate(IPeerConnection peerConnection, RTCIceCandidate candidate)
        {
            if(peerConnection is null)
                throw new ArgumentNullException(nameof(peerConnection));
            ThrowWhenInvalidIceCandidate(candidate);
            _negotiationQueue.Enqueue(new IceCandidateMessage(peerConnection, candidate, false));
        }

        public void EnqueueRemoteAnswer(
            IPeerConnection peerConnection,
            Guid offerId,
            RTCSessionDescription remoteSessionDescription)
        {
            if(peerConnection is null)
                throw new ArgumentNullException(nameof(peerConnection));
            ThrowWhenInvalidSessionDescription(remoteSessionDescription);

            _negotiationQueue.Enqueue(new SdpAnswerMessage(peerConnection, remoteSessionDescription, offerId));
        }

        public void EnqueueRemoteIceCandidate(IPeerConnection peerConnection, RTCIceCandidate candidate)
        {
            if(peerConnection is null)
                throw new ArgumentNullException(nameof(peerConnection));
            ThrowWhenInvalidIceCandidate(candidate);

            _negotiationQueue.Enqueue(new IceCandidateMessage(peerConnection, candidate, true));
        }

        public void EnqueueRemoteOffer(
            IPeerConnection peerConnection,
            RTCSessionDescription remoteSessionDescription)
        {
            if(peerConnection is null)
                throw new ArgumentNullException(nameof(peerConnection));
            ThrowWhenInvalidSessionDescription(remoteSessionDescription);

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

            // Validate TransceiverMetadata
            foreach(var message in transceiverMetadataMessage)
            {
                if(string.IsNullOrWhiteSpace(message.TransceiverMid))
                    throw new ArgumentException($"{nameof(message.TransceiverMid)} is NULL or empty");
            }

            _negotiationQueue.Enqueue(new RemoteTransceiverMetadataMessage(transceiverMetadataMessage, peerConnection));
        }

        public void EnqueueLocalTransceiverMetadata(
            IPeerConnection peerConnection,
            TransceiverMetadata transceiverMetadata)
        {
            if(peerConnection is null)
                throw new ArgumentNullException(nameof(peerConnection));
            if(transceiverMetadata is null)
                throw new ArgumentNullException(nameof(transceiverMetadata));

            _negotiationQueue.Enqueue(new LocalTransceiverMetadataMessage(transceiverMetadata, peerConnection));
        }

        public void EnqueueRenegotiationRequest(IPeerConnection peerConnection)
        {
            if(peerConnection is null)
                throw new ArgumentNullException(nameof(peerConnection));

            _negotiationQueue.Enqueue(new RenegotiationMessage(peerConnection));
        }

        static void ThrowWhenInvalidSessionDescription(RTCSessionDescription remoteSessionDescription)
        {
            if(string.IsNullOrWhiteSpace(remoteSessionDescription.Sdp))
                throw new ArgumentException($"{nameof(remoteSessionDescription.Sdp)} cannot be null or empty");
            if(string.IsNullOrWhiteSpace(remoteSessionDescription.Type))
                throw new ArgumentException($"{nameof(remoteSessionDescription.Type)} cannot be null or empty");
        }

        static void ThrowWhenInvalidIceCandidate(RTCIceCandidate candidate)
        {
            if(string.IsNullOrWhiteSpace(candidate.Candidate))
                throw new ArgumentException($"{nameof(candidate.Candidate)} cannot be null or empty");
            if(string.IsNullOrWhiteSpace(candidate.SdpMid))
                throw new ArgumentException($"{nameof(candidate.SdpMid)} cannot be null or empty");
        }
    }
}

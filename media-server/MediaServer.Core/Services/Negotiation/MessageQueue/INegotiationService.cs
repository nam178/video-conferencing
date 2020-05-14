using MediaServer.Common.Media;
using MediaServer.Core.Models;
using MediaServer.WebRtc.Managed;
using System;
using System.Collections.Generic;

namespace MediaServer.Core.Services.Negotiation.MessageQueue
{
    public interface INegotiationService
    {
        /// <summary>
        /// Inform the negotation service that we received transceiver metadata from the client.
        /// </summary>
        void EnqueueRemoteTransceiverMetadata(
            IPeerConnection peerConnection,
            IReadOnlyList<TransceiverMetadata> transceiverMetadataMessage);

        /// <summary>
        /// Inform the negotiation service that we received remote offer.
        /// Must be called before RemoteIceCandidateReceived() is called, because offer must be queued
        /// and processed before ICE candidates, otherwise libWebRTC will throw errors.
        /// </summary>
        /// <param name="peerConnection">The PeerConnection in which the offer is for</param>
        /// <param name="remoteSessionDescription"></param>
        void EnqueueRemoteOffer(
            IPeerConnection peerConnection,
            RTCSessionDescription remoteSessionDescription);

        /// <summary>
        /// Inform the negotiation service that we received an answer for the offer we've made.
        /// </summary>
        /// <param name="peerConnection"></param>
        /// <param name="offerId"></param>
        /// <param name="remoteSessionDescription"></param>
        void EnqueueRemoteAnswer(
            IPeerConnection peerConnection,
            Guid offerId,
            RTCSessionDescription remoteSessionDescription
            );

        /// <summary>
        /// Inform the negotiation service that we received remote ICE candidate.
        /// Must be called after RemoteIceCandidateReceived() is called.
        /// 
        /// It is important to call this method, instead of directly add ICE candidate into PeerConnection,
        /// because adding candidates directly MAY executed before SetRemoteSessionDescription(),
        /// as SetRemoteSessionDescription() scheduled to run on a queue.
        /// </summary>
        /// <param name="peerConnection"></param>
        /// <param name="candidate"></param>
        void EnqueueRemoteIceCandidate(
            IPeerConnection peerConnection, RTCIceCandidate candidate);

        /// <summary>
        /// Inform the negotiation service that we generated a local ICE candidate, ready to send.
        /// 
        /// Don't send the ICE candidate directly, but let this service to schedule it instead. Otherwise
        /// ICE candidates will be sent and processed by the client before sdp is sent, causing
        /// errors from the client side.
        /// </summary>
        /// <param name="peerConnection"></param>
        /// <param name="candidate"></param>
        void EnqueueLocalIceCandidate(
            IPeerConnection peerConnection, RTCIceCandidate candidate);

        /// <summary>
        /// Inform the negotiation service that the provided PeerConnection requires re-negotiation
        /// </summary>
        /// <param name="peerConnection"></param>
        void EnqueueRenegotiationRequest(IPeerConnection peerConnection);
    }
}

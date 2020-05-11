﻿using MediaServer.Core.Models;
using MediaServer.WebRtc.Managed;

namespace MediaServer.Core.Services.Negotiation.MessageQueue
{
    public interface INegotiationService
    {
        /// <summary>
        /// Inform the negotiation service that we received remote SDP, could be either offer or answer.
        /// Must be called before RemoteIceCandidateReceived() is called.
        /// </summary>
        /// <param name="peerConnection">The PeerConnection in which the offer is for</param>
        /// <param name="remoteSessionDescription"></param>
        void EnqueueRemoteSdpMessage(
            IPeerConnection peerConnection,
            RTCSessionDescription remoteSessionDescription);

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
        /// Don't send the ICE candidate directly, but let this service to schedule it. At the
        /// moment there is no real advantages of doing it via this service yet, other than
        /// keeping it consistent with RemoteIceCandidateReceived();
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

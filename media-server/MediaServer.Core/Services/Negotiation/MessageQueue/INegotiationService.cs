using MediaServer.Core.Models;
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
        void RemoteSessionDescriptionReceived(
            IPeerConnection peerConnection,
            RTCSessionDescription remoteSessionDescription);

        /// <summary>
        /// Inform the negotiation service that we received remote ICE candidate.
        /// Must be called after RemoteIceCandidateReceived() is called.
        /// </summary>
        /// <param name="peerConnection"></param>
        /// <param name="candidate"></param>
        void RemoteIceCandidateReceived(
            IPeerConnection peerConnection, RTCIceCandidate candidate);

        /// <summary>
        /// Inform the negotiation service that the provided PeerConnection requires re-negotiation
        /// </summary>
        /// <param name="peerConnection"></param>
        void RenegotiationRequired(IPeerConnection peerConnection);
    }
}

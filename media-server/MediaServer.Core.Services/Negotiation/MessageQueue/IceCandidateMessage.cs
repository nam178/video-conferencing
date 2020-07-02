using MediaServer.Core.Models;
using MediaServer.WebRtc.Common;

namespace MediaServer.Core.Services.Negotiation.MessageQueue
{
    sealed class IceCandidateMessage : Message
    {
        /// <summary>
        /// The candidate
        /// </summary>
        public RTCIceCandidate Candidate { get; }

        /// <summary>
        /// Whenever this is locally generated ICE candidate, or received from remote
        /// </summary>
        public bool IsRemote { get; }

        public IceCandidateMessage(IPeerConnection peerConnection, RTCIceCandidate candidate, bool isRemote)
            : base(peerConnection)
        {
            Candidate = candidate;
            IsRemote = isRemote;
        }
    }
}
using MediaServer.WebRtc.Managed;
using System;

namespace MediaServer.Rtc.Services
{
    public sealed class PeerConnectionRequest
    {
        /// <summary>
        /// Id that will be set for the new PeerConnection,
        /// if no PeerConection with this id exists for the current user.
        /// </summary>
        public Guid PeerConnectionId { get; set; }

        /// <summary>
        /// The SDP offer
        /// </summary>
        public RTCSessionDescription OfferedSessionDescription { get; set;  }
    }
}
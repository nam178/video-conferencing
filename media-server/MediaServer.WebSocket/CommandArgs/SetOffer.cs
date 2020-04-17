using MediaServer.WebRtc.Managed;
using System;

namespace MediaServer.WebSocket.CommandArgs
{
    public sealed class SetOffer
    {
        /// <summary>
        /// Id of the PeerConnection in which this offer is for.
        /// If no such PeerConnection exists with this id, the server creates new PeerConnection.
        /// </summary>
        public Guid PeerConnectionId { get; set; }

        /// <summary>
        /// The client's session description that it is offering
        /// </summary>
        public RTCSessionDescription Sdp { get; set; }
    }
}

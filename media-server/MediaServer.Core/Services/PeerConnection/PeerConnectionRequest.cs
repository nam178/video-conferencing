using MediaServer.WebRtc.Managed;

namespace MediaServer.Core.Services.PeerConnection
{
    public sealed class PeerConnectionRequest
    {
        /// <summary>
        /// The SDP offer
        /// </summary>
        public RTCSessionDescription OfferedSessionDescription { get; set; }
    }
}
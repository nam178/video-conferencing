using MediaServer.WebRtc.Managed;

namespace MediaServer.Core.Rtc.Services
{
    public sealed class PeerConnectionRequest
    {
        /// <summary>
        /// The SDP offer
        /// </summary>
        public RTCSessionDescription OfferedSessionDescription { get; set; }
    }
}
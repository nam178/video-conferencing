using MediaServer.Core.Models;
using MediaServer.WebRtc.Managed;

namespace MediaServer.Rtc.Services
{
    public sealed class PeerConnectionRequest
    {
        /// <summary>
        /// The SDP offer
        /// </summary>
        public RTCSessionDescription OfferedSessionDescription { get; set;  }
    }
}
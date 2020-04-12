using MediaServer.Models;
using MediaServer.WebRtc.Managed;

namespace MediaServer.WebSocket.CommandHandlers
{
    sealed class SetOffer
    {
        public RTCSessionDescription Offer { get; set; }
    }
}

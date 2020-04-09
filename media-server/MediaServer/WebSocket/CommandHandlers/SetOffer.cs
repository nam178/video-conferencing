using MediaServer.Models;

namespace MediaServer.WebSocket.CommandHandlers
{
    sealed class SetOffer
    {
        public RtcSessionDescription Offer { get; set; }
    }
}

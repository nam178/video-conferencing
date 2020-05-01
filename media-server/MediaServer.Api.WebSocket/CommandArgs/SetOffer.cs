using MediaServer.WebRtc.Managed;

namespace MediaServer.Api.WebSocket.CommandArgs
{
    public sealed class SetOffer
    {
        public RTCSessionDescription Offer { get; set; }
    }
}

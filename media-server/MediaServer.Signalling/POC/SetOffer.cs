using MediaServer.WebRtc.Managed;

namespace MediaServer.Signalling.CommandArgs
{
    sealed class SetOffer
    {
        public RTCSessionDescription Offer { get; set; }
    }
}

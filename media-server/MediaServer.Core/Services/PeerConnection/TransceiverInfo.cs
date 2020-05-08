using MediaServer.WebRtc.Managed;
using MediaServer.WebRtc.MediaRouting;

namespace MediaServer.Core.Services.PeerConnection
{
    public sealed class TransceiverInfo
    {
        public string TransceiverMid { get; set; }

        public MediaQuality TrackQuality { get; set; }

        public MediaKind Kind { get; set; }
    }
}

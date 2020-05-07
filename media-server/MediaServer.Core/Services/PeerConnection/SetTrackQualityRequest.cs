using MediaServer.WebRtc.Managed;
using MediaServer.WebRtc.MediaRouting;

namespace MediaServer.Core.Services.PeerConnection
{
    public sealed class SetTrackQualityRequest
    {
        public string TrackId { get; set; }

        public MediaQuality TrackQuality { get; set; }

        public MediaStreamTrack.TrackKind Kind { get; set; }
    }
}

using MediaServer.WebRtc.Managed;
using MediaServer.WebRtc.MediaRouting;

namespace MediaServer.Core.Services.PeerConnection
{
    public sealed class SetTrackQualityRequest
    {
        public string TrackId { get; set; }

        public TrackQuality TrackQuality { get; set; }

        public MediaStreamTrack.Kind Kind { get; set; }
    }
}

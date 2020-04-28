using MediaServer.WebRtc.Managed;

namespace MediaServer.Core.Services.PeerConnection
{
    public sealed class SetTrackQualityRequest
    {
        public string TrackId { get; set; }

        public TrackQuality TrackQuality { get; set; }
    }
}

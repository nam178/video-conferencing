using MediaServer.Core.Models;

namespace MediaServer.Core.Services.PeerConnection
{
    public sealed class SetTrackQualityRequest
    {
        public string TrackId { get; set; }

        public TrackQuality TrackQuality { get; set; }
    }
}

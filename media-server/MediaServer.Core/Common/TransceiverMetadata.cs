using MediaServer.WebRtc.Managed;
using MediaServer.WebRtc.MediaRouting;

namespace MediaServer.Core.Common
{
    public sealed class TransceiverMetadata
    {
        public string TransceiverMid { get; set; }

        public MediaQuality TrackQuality { get; set; }

        public MediaKind Kind { get; set; }
    }
}

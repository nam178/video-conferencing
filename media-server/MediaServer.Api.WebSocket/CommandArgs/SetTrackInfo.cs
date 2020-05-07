using MediaServer.WebRtc.Managed;
using MediaServer.WebRtc.MediaRouting;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MediaServer.Api.WebSocket.CommandArgs
{
    sealed class SetTrackInfo
    {
        public string TrackId { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public MediaQuality Quality { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public MediaStreamTrack.TrackKind Kind { get; set; }
    }
}
using MediaServer.WebRtc.Managed;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MediaServer.WebSocket.CommandArgs
{
    sealed class SetTrackInfo
    {
        public string TrackId { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public TrackQuality Quality { get; set; }
    }
}
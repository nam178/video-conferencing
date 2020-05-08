using MediaServer.WebRtc.Managed;
using MediaServer.WebRtc.MediaRouting;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MediaServer.Api.WebSocket.CommandArgs
{
    sealed class SetTransceivers
    {
        public TransceiverInfo[] Transceivers { get; set; }

        public sealed class TransceiverInfo
        {
            public string TransceiverMid { get; set; }

            [JsonConverter(typeof(StringEnumConverter))]
            public MediaQuality Quality { get; set; }

            [JsonConverter(typeof(StringEnumConverter))]
            public MediaKind Kind { get; set; }
        }
    }
}
using MediaServer.Core.Common;
using MediaServer.WebRtc.Managed;
using MediaServer.WebRtc.MediaRouting;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MediaServer.Api.WebSocket.CommandArgs
{
    sealed class SetTransceiversMetadata
    {
        public Transceiver[] Transceivers { get; set; }

        public sealed class Transceiver
        {
            public string TransceiverMid { get; set; }

            [JsonConverter(typeof(StringEnumConverter))]
            public MediaQuality Quality { get; set; }

            [JsonConverter(typeof(StringEnumConverter))]
            public MediaKind Kind { get; set; }

            public static implicit operator TransceiverMetadata(Transceiver transceiverInfo)
            {
                return new TransceiverMetadata
                {
                    Kind = transceiverInfo.Kind,
                    TrackQuality = transceiverInfo.Quality,
                    TransceiverMid = transceiverInfo.TransceiverMid
                };
            }
        }
    }
}
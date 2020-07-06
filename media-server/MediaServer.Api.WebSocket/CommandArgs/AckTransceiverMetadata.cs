using MediaServer.Common.Media;

namespace MediaServer.Api.WebSocket.CommandArgs
{
    sealed class AckTransceiverMetadata
    {
        public TransceiverMetadata Acked { get; set; }
    }
}

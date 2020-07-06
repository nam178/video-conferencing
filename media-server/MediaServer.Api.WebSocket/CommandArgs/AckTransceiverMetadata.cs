using MediaServer.Api.WebSocket.Models;

namespace MediaServer.Api.WebSocket.CommandArgs
{
    sealed class AckTransceiverMetadata
    {
        public WsTransceiverMetadata Acked { get; set; }
    }
}

using MediaServer.Api.WebSocket.CommandArgs;
using MediaServer.Api.WebSocket.Net;
using MediaServer.Common.Patterns;
using MediaServer.Core.Services.Negotiation.Handlers;
using System.Threading.Tasks;

namespace MediaServer.Api.WebSocket.CommandHandlers
{
    sealed class AckTransceiverMetadataCommandHandler : IHandler<IWebSocketRemoteDevice, AckTransceiverMetadata>
    {
        readonly IAckTransceiverMetadataHandler _handler;

        public AckTransceiverMetadataCommandHandler(IAckTransceiverMetadataHandler handler)
        {
            _handler = handler
                ?? throw new System.ArgumentNullException(nameof(handler));
        }

        public Task HandleAsync(IWebSocketRemoteDevice remoteDevice, AckTransceiverMetadata arg2)
        {
            _handler.Handle(remoteDevice, arg2.Acked);
            return Task.CompletedTask;
        }
    }
}

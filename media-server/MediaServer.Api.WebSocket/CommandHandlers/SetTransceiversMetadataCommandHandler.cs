using MediaServer.Api.WebSocket.CommandArgs;
using MediaServer.Api.WebSocket.Net;
using MediaServer.Common.Mediator;
using MediaServer.Common.Utils;
using MediaServer.Core.Common;
using MediaServer.Core.Services.PeerConnection;
using System.Linq;
using System.Threading.Tasks;

namespace MediaServer.Api.WebSocket.CommandHandlers
{
    sealed class SetTransceiversMetadataCommandHandler : IHandler<IWebSocketRemoteDevice, CommandArgs.SetTransceiversMetadata>
    {
        readonly ITransceiverMetadataSetter _service;

        public SetTransceiversMetadataCommandHandler(ITransceiverMetadataSetter service)
        {
            _service = service ?? throw new System.ArgumentNullException(nameof(service));
        }

        public async Task HandleAsync(IWebSocketRemoteDevice remoteDevice, SetTransceiversMetadata args)
        {
            Require.NotNull(args);
            Require.NotNull(args.Transceivers);

            await _service.HandleAsync(remoteDevice, args.Transceivers.Select(trans => (TransceiverMetadata)trans).ToArray());
            await remoteDevice.SendAsync("TransceiversMetadataSet", null);
        }
    }
}

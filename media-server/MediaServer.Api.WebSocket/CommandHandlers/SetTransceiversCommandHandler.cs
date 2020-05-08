using MediaServer.Api.WebSocket.CommandArgs;
using MediaServer.Api.WebSocket.Net;
using MediaServer.Common.Mediator;
using MediaServer.Common.Utils;
using MediaServer.Core.Services.PeerConnection;
using System.Threading.Tasks;

namespace MediaServer.Api.WebSocket.CommandHandlers
{
    sealed class SetTransceiversCommandHandler : IHandler<IWebSocketRemoteDevice, CommandArgs.SetTransceivers>
    {
        readonly ITransceiverInfoSetter _service;

        public SetTransceiversCommandHandler(ITransceiverInfoSetter service)
        {
            _service = service ?? throw new System.ArgumentNullException(nameof(service));
        }

        public async Task HandleAsync(IWebSocketRemoteDevice remoteDevice, SetTransceivers args)
        {
            Require.NotNull(args);
            Require.NotNull(args.Transceivers);

            await _service.HandleAsync(remoteDevice, args.Transceivers);
            await remoteDevice.SendAsync("TransceiversInfoSet", null);
        }
    }
}

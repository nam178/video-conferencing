using MediaServer.Common.Mediator;
using MediaServer.Common.Utils;
using MediaServer.Models;
using MediaServer.WebRtc.Managed;
using MediaServer.Api.WebSocket.CommandArgs;
using MediaServer.Api.WebSocket.Net;
using System.Threading.Tasks;

namespace MediaServer.Api.WebSocket.CommandHandlers
{
    sealed class AddIceCandidateHandler : IHandler<IWebSocketRemoteDevice, CommandArgs.AddIceCandidate>
    {
        readonly IHandler<IRemoteDevice, RTCIceCandidate> _iceCandidateHandler;

        public AddIceCandidateHandler(IHandler<IRemoteDevice, RTCIceCandidate> iceCandidateHandler)
        {
            _iceCandidateHandler = iceCandidateHandler
                ?? throw new System.ArgumentNullException(nameof(iceCandidateHandler));
        }

        public Task HandleAsync(IWebSocketRemoteDevice remoteDeivce, AddIceCandidate args)
        {
            Require.NotNull(args.Candidate.Candidate);
            Require.NotNull(args.Candidate.SdpMid);

            // This command has no response, 
            // just pass direcrlt through the service to handle it
            return _iceCandidateHandler.HandleAsync(remoteDeivce, args.Candidate);
        }
    }
}


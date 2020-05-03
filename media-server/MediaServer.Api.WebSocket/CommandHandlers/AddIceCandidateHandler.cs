using MediaServer.Api.WebSocket.CommandArgs;
using MediaServer.Api.WebSocket.Net;
using MediaServer.Common.Mediator;
using MediaServer.Common.Utils;
using MediaServer.Core.Services.PeerConnection;
using System.Threading.Tasks;

namespace MediaServer.Api.WebSocket.CommandHandlers
{
    sealed class AddIceCandidateHandler : IHandler<IWebSocketRemoteDevice, CommandArgs.AddIceCandidate>
    {
        readonly IRTCIceCandidateHandler _iceCandidateHandler;

        public AddIceCandidateHandler(IRTCIceCandidateHandler iceCandidateHandler)
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
            return _iceCandidateHandler.AddCandidateAsync(remoteDeivce, args.PeerConnectionId, args.Candidate);
        }
    }
}


using MediaServer.Common.Mediator;
using MediaServer.Common.Utils;
using MediaServer.Core.Services;
using MediaServer.WebRtc.Managed;
using MediaServer.WebSocket.CommandArgs;
using MediaServer.WebSocket.Net;
using System.Threading.Tasks;

namespace MediaServer.WebSocket.CommandHandlers
{
    sealed class AddIceCandidateHandler : IHandler<IWebSocketRemoteDevice, CommandArgs.AddIceCandidate>
    {
        readonly IRemoteDeviceService<RTCIceCandidate> _iceCandidateHandler;

        public AddIceCandidateHandler(IRemoteDeviceService<RTCIceCandidate> iceCandidateHandler)
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


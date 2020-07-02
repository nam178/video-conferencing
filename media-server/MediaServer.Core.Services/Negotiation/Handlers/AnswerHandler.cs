using MediaServer.Common.Utils;
using MediaServer.Core.Models;
using MediaServer.Core.Services.Negotiation.MessageQueue;
using MediaServer.WebRtc.Common;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MediaServer.Core.Services.Negotiation.Handlers
{
    sealed class AnswerHandler : IAnswerHandler
    {
        readonly INegotiationService _negotiationService;

        public AnswerHandler(INegotiationService negotiationService)
        {
            _negotiationService = negotiationService 
                ?? throw new ArgumentNullException(nameof(negotiationService));
        }

        public Task HandleAsync(IRemoteDevice remoteDevice, Guid peerConnectionId, Guid offerId, RTCSessionDescription answer)
        {
            Require.NotNull(answer.Sdp);
            Require.NotNull(answer.Type);
            Require.NotEmpty(offerId);
            Require.NotEmpty(peerConnectionId);

            // Get user and current IPeerConnection for this device
            var deviceData = remoteDevice.GetCustomData();
            if(null == deviceData.User)
                throw new UnauthorizedAccessException();

            // Get the PeerConnection that this answer is for
            var peerConnection = deviceData.PeerConnections.FirstOrDefault(p => p.Id == peerConnectionId);
            if(null == peerConnection)
                throw new InvalidOperationException($"PeerConnection {peerConnectionId} does not exist for the device {remoteDevice}");

            _negotiationService.EnqueueRemoteAnswer(peerConnection, offerId, answer);
            return Task.CompletedTask;
        }
    }
}

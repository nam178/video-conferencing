using MediaServer.Common.Utils;
using MediaServer.Models;
using MediaServer.WebRtc.Managed;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MediaServer.Core.Services.Negotiation.Handlers
{
    sealed class AnswerHandler : IAnswerHandler
    {
        public Task HandleAsync(IRemoteDevice remoteDevice, Guid peerConnectionId, RTCSessionDescription answer)
        {
            Require.NotNull(answer.Sdp);
            Require.NotNull(answer.Type);

            // Get user and current IPeerConnection for this device
            var deviceData = remoteDevice.GetCustomData();
            if(null == deviceData.User)
                throw new UnauthorizedAccessException();

            // Get the PeerConnection that this answer is for
            var peerConnection = deviceData.PeerConnections.FirstOrDefault(p => p.Id == peerConnectionId);
            if(null == peerConnection)
                throw new InvalidOperationException($"PeerConnection {peerConnectionId} does not exist for the device {remoteDevice}");

            deviceData.User.Room.NegotiationService.EnqueueRemoteSdpMessage(peerConnection, answer);
            return Task.CompletedTask;
        }
    }
}

using MediaServer.Core.Models;
using MediaServer.Core.Services.Negotiation.MessageQueue;
using MediaServer.WebRtc.Common;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MediaServer.Core.Services.Negotiation.Handlers
{
    sealed class IceCandidateHandler : IIceCandidateHandler
    {
        readonly INegotiationService _negotiationService;

        public IceCandidateHandler(INegotiationService negotiationService)
        {
            _negotiationService = negotiationService
                ?? throw new ArgumentNullException(nameof(negotiationService));
        }

        public Task AddAsync(IRemoteDevice remoteDevice, Guid peerConnectionId, RTCIceCandidate iceCandidate)
        {
            var peerConnection = remoteDevice.GetCustomData().PeerConnections.FirstOrDefault(p => p.Id == peerConnectionId);
            if(null == peerConnection)
            {
                throw new InvalidOperationException(
                    $"Could not find PeerConnection with Id {peerConnectionId} in {remoteDevice}"
                    );
            }

            if(null == peerConnection.Room)
            {
                throw new UnauthorizedAccessException(
                    $"Device must join a room first before sending ICE candidates");
            }

            _negotiationService.EnqueueRemoteIceCandidate(peerConnection, iceCandidate);
            return Task.CompletedTask;
        }
    }
}

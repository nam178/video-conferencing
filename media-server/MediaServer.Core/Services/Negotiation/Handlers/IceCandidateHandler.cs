using MediaServer.Models;
using MediaServer.WebRtc.Managed;
using NLog;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MediaServer.Core.Services.Negotiation.Handlers
{
    sealed class IceCandidateHandler : IIceCandidateHandler
    {
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
                throw new InvalidProgramException();
            }

            peerConnection.Room.NegotiationService.RemoteIceCandidateReceived(peerConnection, iceCandidate);
            return Task.CompletedTask;
        }
    }
}

using MediaServer.Models;
using MediaServer.WebRtc.Managed;
using System;
using System.Threading.Tasks;

namespace MediaServer.Core.Services.PeerConnection
{
    public interface IRTCIceCandidateHandler
    {
        Task AddCandidateAsync(IRemoteDevice remoteDevice, Guid peerConnectionId, RTCIceCandidate iceCandidate);
    }
}

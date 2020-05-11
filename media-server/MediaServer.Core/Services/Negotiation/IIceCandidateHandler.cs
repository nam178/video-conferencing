using MediaServer.Models;
using MediaServer.WebRtc.Managed;
using System;
using System.Threading.Tasks;

namespace MediaServer.Core.Services.Negotiation
{
    public interface IIceCandidateHandler
    {
        Task AddAsync(IRemoteDevice remoteDevice, Guid peerConnectionId, RTCIceCandidate iceCandidate);
    }
}

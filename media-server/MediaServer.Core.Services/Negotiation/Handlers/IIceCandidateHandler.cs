using MediaServer.Core.Models;
using MediaServer.WebRtc.Common;
using System;
using System.Threading.Tasks;

namespace MediaServer.Core.Services.Negotiation.Handlers
{
    public interface IIceCandidateHandler
    {
        Task AddAsync(IRemoteDevice remoteDevice, Guid peerConnectionId, RTCIceCandidate iceCandidate);
    }
}

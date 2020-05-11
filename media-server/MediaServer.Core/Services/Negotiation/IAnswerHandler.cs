using MediaServer.Models;
using MediaServer.WebRtc.Managed;
using System;
using System.Threading.Tasks;

namespace MediaServer.Core.Services.Negotiation
{
    public interface IAnswerHandler
    {
        Task HandleAsync(IRemoteDevice remoteDevice, Guid peerConnectionId, RTCSessionDescription answer);
    }
}
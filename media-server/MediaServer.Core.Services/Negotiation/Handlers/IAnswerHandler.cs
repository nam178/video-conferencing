using MediaServer.Models;
using MediaServer.WebRtc.Common;
using System;
using System.Threading.Tasks;

namespace MediaServer.Core.Services.Negotiation.Handlers
{
    public interface IAnswerHandler
    {
        Task HandleAsync(IRemoteDevice remoteDevice, Guid peerConnectionId, Guid offerId, RTCSessionDescription answer);
    }
}
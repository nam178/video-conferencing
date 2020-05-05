using MediaServer.Models;
using MediaServer.WebRtc.Managed;
using System;
using System.Threading.Tasks;

namespace MediaServer.Core.Services.PeerConnection
{
    public interface IAnswerHandler
    {
        Task HandleAsync(IRemoteDevice remoteDevice, Guid peerConnectionId, RTCSessionDescription answer);
    }
}
using MediaServer.Common.Mediator;
using MediaServer.Models;
using MediaServer.WebRtc.Managed;
using System.Threading.Tasks;

namespace MediaServer.Core.Services.PeerConnection
{
    public interface IRTCSessionDescriptionHandler
    {
        Task ReceiveSdpAsync(IRemoteDevice remoteDevice, RTCSessionDescription request);
    }
}

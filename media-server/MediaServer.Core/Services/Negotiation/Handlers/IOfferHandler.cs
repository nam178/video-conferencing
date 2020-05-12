using MediaServer.Common.Media;
using MediaServer.Models;
using MediaServer.WebRtc.Managed;
using System;
using System.Threading.Tasks;

namespace MediaServer.Core.Services.Negotiation.Handlers
{
    public interface IOfferHandler
    {
        Task HandleAsync(
            IRemoteDevice remoteDevice,
            Guid? peerConnectionId,
            RTCSessionDescription request,
            TransceiverMetadata[] transceivers);
    }
}

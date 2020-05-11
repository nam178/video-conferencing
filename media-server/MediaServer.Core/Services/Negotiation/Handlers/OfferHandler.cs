using MediaServer.Common.Utils;
using MediaServer.Core.Models;
using MediaServer.Models;
using MediaServer.WebRtc.Managed;
using NLog;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MediaServer.Core.Services.Negotiation.Handlers
{
    sealed class OfferHandler : IOfferHandler
    {
        readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        public Task HandleAsync(IRemoteDevice remoteDevice, Guid? peerConnectionId, RTCSessionDescription offer)
        {
            Require.NotNull(offer.Sdp);
            Require.NotNull(offer.Type);

            // Ensure the user has logged in
            var deviceData = remoteDevice.GetCustomData();
            if(null == deviceData.User)
            {
                throw new UnauthorizedAccessException();
            }

            // Get or create new PeerConnection, depending on the client's requests
            IPeerConnection peerConnection = null;
            if(peerConnectionId != null)
            {
                peerConnection = deviceData.PeerConnections.First(p => p.Id == peerConnectionId.Value);
            }
            else
            {
                if(deviceData.PeerConnections.Count > 3)
                    throw new InvalidOperationException($"Max 3 PeerConnection allowed per device");
                peerConnection = CreatePeerConnection(remoteDevice, deviceData.User, offer);
            }

            // Save
            deviceData.PeerConnections.Add(peerConnection);
            remoteDevice.SetCustomData(deviceData);

            // Let the negotiation service handle the rest
            deviceData.User.Room.NegotiationService.EnqueueRemoteSdpMessage(peerConnection, offer);
            return Task.CompletedTask;
        }

        IPeerConnection CreatePeerConnection(IRemoteDevice remoteDevice, User user, RTCSessionDescription remoteSdp)
        {
            var peerConnection = user.Room.CreatePeerConnection(remoteDevice);
            _logger.Info($"PeerConnection created, user {user}, device {remoteDevice}");

            // This is the first time is PeerConnection is created,
            // we'll add ICE candidate observer
            peerConnection
                .ObserveIceCandidate((peer, candidate) => user.Room.NegotiationService.EnqueueLocalIceCandidate(peerConnection, candidate))
                .ObserveRenegotiationNeeded(peer => user.Room.NegotiationService.EnqueueRenegotiationRequest(peerConnection));

            return peerConnection;
        }

    }
}

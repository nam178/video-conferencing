using MediaServer.Common.Media;
using MediaServer.Common.Utils;
using MediaServer.Core.Models;
using MediaServer.Core.Services.Negotiation.MessageQueue;
using MediaServer.WebRtc.Common;
using NLog;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MediaServer.Core.Services.Negotiation.Handlers
{
    sealed class OfferHandler : IOfferHandler
    {
        readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        readonly INegotiationService _negotiationService;

        public OfferHandler(INegotiationService negotiationService)
        {
            _negotiationService = negotiationService 
                ?? throw new ArgumentNullException(nameof(negotiationService));
        }

        public Task HandleAsync(
            IRemoteDevice remoteDevice,
            Guid? peerConnectionId,
            RTCSessionDescription offer,
            TransceiverMetadata[] transceivers)
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
                peerConnection = CreatePeerConnection(remoteDevice, deviceData.User);
            }

            // Save
            deviceData.PeerConnections.Add(peerConnection);
            remoteDevice.SetCustomData(deviceData);

            // Let the negotiation service handle the rest
            // Notes - queue transceiver metadata first before queuing the remote sdp,
            // so at the time processing remote sdp (which has remote transceivers) 
            // we know transceviers' metadata inhand.
            // Fail to do so will cause errors.
            _negotiationService.EnqueueRemoteTransceiverMetadata(peerConnection, transceivers);
            _negotiationService.EnqueueRemoteOffer(peerConnection, offer);
            return Task.CompletedTask;
        }

        IPeerConnection CreatePeerConnection(IRemoteDevice remoteDevice, User user)
        {
            var peerConnection = user.Room.CreatePeerConnection(remoteDevice);
            _logger.Info($"PeerConnection created, user {user}, device {remoteDevice}");

            // This is the first time is PeerConnection is created,
            // we'll add ICE candidate observer
            peerConnection
                .ObserveIceCandidate((peer, candidate) => _negotiationService.EnqueueLocalIceCandidate(peerConnection, candidate))
                .ObserveRenegotiationNeeded(peer => _negotiationService.EnqueueRenegotiationRequest(peerConnection));

            return peerConnection;
        }

    }
}

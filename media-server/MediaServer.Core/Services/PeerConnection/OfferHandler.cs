using MediaServer.Common.Utils;
using MediaServer.Core.Models;
using MediaServer.Models;
using MediaServer.WebRtc.Managed;
using NLog;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MediaServer.Core.Services.PeerConnection
{
    sealed class OfferHandler : IOfferHandler
    {
        readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        readonly IRenegotationHandler _renegotationHandler;

        public OfferHandler(IRenegotationHandler renegotationHandler)
        {
            _renegotationHandler = renegotationHandler
                ?? throw new ArgumentNullException(nameof(renegotationHandler));
        }

        public async Task HandleAsync(IRemoteDevice remoteDevice, Guid? peerConnectionId, RTCSessionDescription offer)
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
                peerConnection = await CreatePeerConnection(remoteDevice, deviceData.User, offer);
            }

            // Save
            deviceData.PeerConnections.Add(peerConnection);
            remoteDevice.SetCustomData(deviceData);

            // Then begin negotating
            await deviceData.User.Room.RenegotiationQueue.ExecuteAsync(async delegate
            {
                // Create Answer
                var answer = await peerConnection.CreateAnswerAsync();
                _logger.Info($"Answer {answer} created for {peerConnection}");

                // Send Answer
                remoteDevice.EnqueueSessionDescription(peerConnection.Id, answer);
                _logger.Info($"Answer sent for {peerConnection}");

                // Save the Answer locally
                // SetLocalSessionDescriptionAsync() must be after SendSessionDescriptionAsync()
                // because it SetLocalSessionDescriptionAsync() generates ICE candidates,
                // and we want to send ICE candidates after remote SDP is set.
                await peerConnection.SetLocalSessionDescriptionAsync(answer);
                _logger.Info($"Local description {answer} set for {peerConnection}");
            });
        }

        async Task<IPeerConnection> CreatePeerConnection(IRemoteDevice remoteDevice, User user, RTCSessionDescription remoteSdp)
        {
            var peerConnection = user.Room.CreatePeerConnection(remoteDevice);
            _logger.Info($"PeerConnection created, user {user}, device {remoteDevice}");

            await peerConnection.SetRemoteSessionDescriptionAsync(remoteSdp);
            _logger.Info($"Remote {remoteSdp} SDP set for {peerConnection}");

            // This is the first time is PeerConnection is created,
            // we'll add ICE candidate observer
            peerConnection
                .ObserveIceCandidate((peer, cand) => remoteDevice.EnqueueIceCandidate(peer.Id, cand))
                .ObserveRenegotiationNeeded(async peer => await _renegotationHandler.HandleAsync(remoteDevice, peerConnection));

            return peerConnection;
        }

    }
}

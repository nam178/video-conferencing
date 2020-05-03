﻿using MediaServer.Common.Threading;
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

        public async Task HandleAsync(IRemoteDevice remoteDevice, Guid? peerConnectionId, RTCSessionDescription request)
        {
            Require.NotNull(request.Sdp);
            Require.NotNull(request.Type);

            // Get user and current IPeerConnection for this device
            var deviceData = remoteDevice.GetCustomData();
            if(null == deviceData.User)
            {
                throw new UnauthorizedAccessException();
            }

            // Try to find the existing PeerConnection to update it
            IPeerConnection peerConnection = null;
            if(peerConnectionId != null)
            {
                peerConnection = deviceData.PeerConnections.FirstOrDefault(p => p.Id == peerConnectionId);
            }

            // Create new PeerConnection
            if(null == peerConnection)
            {
               
                peerConnection = await CreatePeerConnection(remoteDevice, deviceData.User, request);
                // Save
                deviceData.PeerConnections.Add(peerConnection);
                remoteDevice.SetCustomData(deviceData);
            }
            // Update existing PeerConnection
            else
            {
                await peerConnection.SetRemoteSessionDescriptionAsync(request);
                _logger.Info($"Remote {request} SDP updated for {peerConnection}");
            }

            // Finally do the SDP exchange, order important
            // Create answer and send it.
            var answer = await peerConnection.CreateAnswerAsync();
            _logger.Info($"Answer {answer} created for {peerConnection}");
            await remoteDevice.SendSessionDescriptionAsync(peerConnection.Id, answer);
            _logger.Info($"Answer sent for {peerConnection}");

            // SetLocalSessionDescriptionAsync() must be after SendSessionDescriptionAsync()
            // because it SetLocalSessionDescriptionAsync() generates ICE candidates,
            // and we want to send ICE candidates after remote SDP is set.
            await peerConnection.SetLocalSessionDescriptionAsync(answer);
            _logger.Info($"Local description {answer} set for {peerConnection}");
        }

        async Task<IPeerConnection> CreatePeerConnection(IRemoteDevice remoteDevice, User user, RTCSessionDescription remoteSdp)
        {
            var peerConnection = user.Room.CreatePeerConnection(remoteDevice);

            await peerConnection.SetRemoteSessionDescriptionAsync(remoteSdp);
            _logger.Info($"Remote {remoteSdp} SDP set for {peerConnection}");

            await peerConnection.StartMediaRoutingAsync();
            _logger.Info($"PeerConnection created, user {user}, device {remoteDevice}");

            // This is the first time is PeerConnection is created,
            // we'll add ICE candidate observer
            peerConnection
                .ObserveIceCandidate((peer, cand) => SendAndForget(remoteDevice, peer, cand))
                .ObserveRenegotiationNeeded(peer =>
                {
                    throw new NotImplementedException();
                });

            return peerConnection;
        }

        static void SendAndForget(IRemoteDevice remoteDevice, IPeerConnection peer, RTCIceCandidate cand)
        {
            remoteDevice
                .SendIceCandidateAsync(peer.Id, cand)
                .Forget($"Error when sending ICE candidate {cand} to device {remoteDevice}");
        }
    }
}

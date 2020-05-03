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
    sealed class RTCSessionDescriptionHandler : IRTCSessionDescriptionHandler
    {
        readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        public async Task ReceiveSdpAsync(IRemoteDevice remoteDevice, RTCSessionDescription request)
        {
            Require.NotNull(request.Sdp);
            Require.NotNull(request.Type);

            // Get user and current IPeerConnection for this device
            var deviceData = remoteDevice.GetCustomData();
            if(null == deviceData.User)
            {
                throw new UnauthorizedAccessException();
            }

            var peerConnection = deviceData.PeerConnections.FirstOrDefault();
            // If no PeerConnection for this device, create one
            if(null == peerConnection)
            {
                peerConnection = await CreatePeerConnection(remoteDevice, deviceData.User);
                // Save
                deviceData.PeerConnections.Add(peerConnection);
                remoteDevice.SetCustomData(deviceData);
            }

            // Finally do the SDP exchange, 
            // it has to be done in this exact oder:
            await peerConnection.SetRemoteSessionDescriptionAsync(request);
            _logger.Info($"Remote {request} SDP set for {peerConnection}");

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

        async Task<IPeerConnection> CreatePeerConnection(IRemoteDevice remoteDevice, User user)
        {
            var peerConnection = user.Room.CreatePeerConnection(remoteDevice);
            await peerConnection.InitialiseAsync();
            _logger.Info($"PeerConnection created, user {user}, device {remoteDevice}");

            // This is the first time is PeerConnection is created,
            // we'll add ICE candidate observer
            peerConnection.ObserveIceCandidate(ice => remoteDevice
                .SendIceCandidateAsync(peerConnection.Id, ice)
                .Forget($"Error when sending ICE candidate {ice} to device {remoteDevice}"));

            return peerConnection;
        }
    }
}

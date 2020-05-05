﻿using MediaServer.Models;
using MediaServer.WebRtc.Managed;
using NLog;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MediaServer.Core.Services.PeerConnection
{
    sealed class IceCandidateHandler : IIceCandidateHandler
    {
        readonly ILogger _logger = NLog.LogManager.GetCurrentClassLogger();

        public Task AddAsync(IRemoteDevice remoteDevice, Guid peerConnectionId, RTCIceCandidate iceCandidate)
        {
            var peerConnection = remoteDevice.GetCustomData().PeerConnections.FirstOrDefault(p => p.Id == peerConnectionId);
            if(null == peerConnection)
            {
                throw new InvalidOperationException(
                    $"Could not find PeerConnection with Id {peerConnectionId} in {remoteDevice}"
                    );
            }
            peerConnection.AddIceCandidate(iceCandidate);
            _logger.Trace($"Ice candidate {iceCandidate} added to {peerConnection}");

            return Task.CompletedTask;
        }
    }
}